using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace fnValidadorBoleto;

public class barcode_validate
{
    private readonly ILogger<barcode_validate> _logger;

    public barcode_validate(ILogger<barcode_validate> logger)
    {
        _logger = logger;
    }

    [Function("barcode_validate")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        string barcodeData = data?.barcode;

        if (string.IsNullOrEmpty(barcodeData))
        {
            _logger.LogError("barcode invlido");
            return new BadRequestObjectResult("o campo barcode é obrigatório");
        }

        if (barcodeData.Length != 44)
        {
            _logger.LogError("barcode inválido");
            var result = new {valido = false, mensagem = "o campo barcode deve conter 44 caracteres"};
            return new BadRequestObjectResult(result);
        }

        string datePart = barcodeData.Substring(3, 8);
        if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dateObj))
        {
            _logger.LogError("data inválida no barcode");
            var result = new {valido = false, mensagem = "data inválida no campo barcode"};
            return new BadRequestObjectResult(result);
        }

        var resultOk = new { valido = true, mensagem = "barcode válido", vencimento = dateObj.ToString("dd/MM/yyyy") };
        return new OkObjectResult(resultOk);
    }
}
