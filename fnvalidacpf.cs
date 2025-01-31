using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChecagemDocumento
{
    public static class CpfFunctions
    {
        [FunctionName("ChecarCPFValido")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest requisicao,
            ILogger log)
        {
            log.LogInformation("Iniciando a checagem do CPF.");

            var conteudo = await new StreamReader(requisicao.Body).ReadToEndAsync();
            dynamic dadosEntrada = JsonConvert.DeserializeObject(conteudo);

            if (dadosEntrada == null || dadosEntrada.cpf == null)
            {
                return new BadRequestObjectResult("Necessário informar o CPF no corpo da requisição.");
            }

            string cpfInformado = dadosEntrada.cpf;

            bool cpfValido = ChecarFormatoCpf(cpfInformado);

            return cpfValido
                ? (IActionResult)new OkObjectResult("CPF válido.")
                : new BadRequestObjectResult("CPF inválido.");
        }

        private static bool ChecarFormatoCpf(string cpf)
        {
            // Confere se foi informado algo e remove caracteres não numéricos
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            var somenteDigitos = new string(cpf.Where(char.IsDigit).ToArray());
            if (somenteDigitos.Length != 11 || somenteDigitos.Distinct().Count() == 1)
                return false;

            // Cálculo do primeiro dígito verificador
            int[] pesos1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int acumulador1 = 0;
            for (int i = 0; i < 9; i++)
            {
                acumulador1 += (somenteDigitos[i] - '0') * pesos1[i];
            }
            int digito1 = acumulador1 % 11 < 2 ? 0 : 11 - (acumulador1 % 11);

            // Cálculo do segundo dígito verificador
            int[] pesos2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int acumulador2 = 0;
            for (int i = 0; i < 10; i++)
            {
                acumulador2 += (somenteDigitos[i] - '0') * pesos2[i];
            }
            int digito2 = acumulador2 % 11 < 2 ? 0 : 11 - (acumulador2 % 11);

            // Confere se os dígitos calculados batem com os da string
            return (somenteDigitos[9] - '0') == digito1 &&
                   (somenteDigitos[10] - '0') == digito2;
        }
    }
}
