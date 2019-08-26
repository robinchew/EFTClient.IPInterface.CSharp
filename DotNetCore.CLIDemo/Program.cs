using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;

class CommandArgs {
    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("ssl")]
    public bool Ssl { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

public delegate void Print(int value);

class EFTClientIPDemo
    {
        ManualResetEvent txnFired = new ManualResetEvent(false);

        public void Run(CommandArgs args)
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIP()
            {
                HostName = args.Host,
                HostPort = args.Port,
                UseSSL = args.Ssl
            };
            // Hook up events
            eft.OnDisplay += delegate(object sender, EFTEventArgs<EFTDisplayResponse> e) {
                foreach(var Str in e.Response.DisplayText) {
                    var isApproved = Str.Trim() == "APPROVED";
                    if (isApproved) {
                        eft.DoSendKey(new EFTSendKeyRequest() {
                            Key = EFTPOSKey.OkCancel
                        });
                    }
                }
            };
            eft.OnReceipt += Eft_OnReceipt;;
            eft.OnTransaction += Eft_OnTransaction;
            eft.OnTerminated += Eft_OnTerminated;
            // Connect
            if (!eft.Connect())
            {
                // Handle failed connection
                Console.WriteLine("Connect failed");
                return;
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set TxnRef to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmtCash for cash out, and AmtPurchase for purchase/refund
                AmtPurchase = args.Amount,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
                // Set application. Used for gift card & 3rd party payment
                Application = TerminalApplication.EFTPOS
            };
            // Send transaction
            if (!eft.DoTransaction(r))
            {
                // Handle failed send
                Console.WriteLine("Send failed");
                return;
            }

            txnFired.WaitOne();
            eft.Disconnect();
            eft.Dispose();
        }

        private void Eft_OnTerminated(object sender, SocketEventArgs e)
        {
            // Handle socket close
            Console.WriteLine($"Socket closed");
            txnFired.Reset();
        }

        private void Eft_OnReceipt(object sender, EFTEventArgs<EFTReceiptResponse> e)
        {
            // Handle receipt
            Console.WriteLine($"{e.Response.Type} receipt");
            Array.ForEach(e.Response.ReceiptText, Console.WriteLine);
        }

        private void Eft_OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
        {
            // Handle transaction event 
            var displayText = e.Response.Success ? "successful" : "unsuccessful";
            Console.WriteLine($"Transaction was {displayText}");
            txnFired.Set();
        }
    }
    
class Program
    {
        static void Main(string[] args)
        {
            (new EFTClientIPDemo()).Run(JsonSerializer.Deserialize<CommandArgs>(args[0]));
        }
    }    
