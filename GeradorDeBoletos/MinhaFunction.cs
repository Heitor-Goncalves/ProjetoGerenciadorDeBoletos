using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using System.Net.Http;
using BarcodeStandard;

namespace GeradorDeBoletos
{
    public class GeradorCodigoBarras
    {
        private readonly ILogger<GeradorCodigoBarras> _logger;
        private readonly string _serviceBusConnectionString;
        private readonly string _queueName = "gerador-codigo-de-barras";
        public GeradorCodigoBarras(ILogger<GeradorCodigoBarras> logger)
        {
            _logger = logger;
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        }
        [FunctionName("barcode-generate")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string valor = data?.valor;
                string dataVencimento = data?.dataVencimento;

                string barcodeData;

                //validação simples dos dados
                if (string.IsNullOrEmpty(valor) || string.IsNullOrEmpty(dataVencimento))
                {
                    return new BadRequestObjectResult("Dados inválidos. Certifique-se de enviar 'valorOriginal' e 'dataVencimento'.");
                }

                //validar formato da data
                if (!DateTime.TryParseExact(dataVencimento, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dateObj))
                {
                    return new BadRequestObjectResult("Formato de data inválido. Use 'yyyy-MM-dd'.");
                }

                string dateStr = dateObj.ToString("yyyyMMdd");

                //conversão do valor para centavos e formatação até  8 digitos
                if (!decimal.TryParse(valor, out decimal valorDecimal) || valorDecimal < 0)
                {
                    return new BadRequestObjectResult("Valor inválido. Certifique-se de enviar um número positivo.");
                }
                int valorCentavos = (int)(valorDecimal * 10);
                string valorStr = valorCentavos.ToString("D8");

                string bankCode = "001"; // Exemplo de código do banco
                string baseCode = string.Concat(bankCode, dateStr, valorStr);
                barcodeData = baseCode.Length < 44 ? baseCode.PadRight(44, '0') : baseCode.Substring(0, 44);

                Barcode barcode = new Barcode();
                var sKImage = barcode.Encode(BarcodeStandard.Type.Code128, barcodeData);
                using (var encondeData = sKImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    var imageBytes = encondeData.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    var resultObject = new
                    {
                        barcode = barcodeData,
                        valorOriginal = valorDecimal,
                        dataVencimento = DateTime.Now.AddDays(5),
                        imagemBase64 = base64String
                    };

                    await SendFileFallBack(resultObject, _serviceBusConnectionString, _queueName);
                    return new OkObjectResult(resultObject);
                }
            }
            catch (Exception)
            {

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

        }

        private async Task SendFileFallBack(object resultObject, string serviceBusConnectionString, string queueName)
        {
            await using var client = new ServiceBusClient(serviceBusConnectionString);
            ServiceBusSender sender = client.CreateSender(queueName);
            string messageBody = JsonConvert.SerializeObject(resultObject);
            ServiceBusMessage message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
            _logger.LogInformation($"Mensagem enviada para a fila {queueName}: {messageBody}");
        }
    }
}
