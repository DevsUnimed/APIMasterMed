using APIMasterMed.Framework;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Data;

namespace APIMasterMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaDigitalController : ControllerBase
    {
        public string ErrorLog { get; private set; } = string.Empty;
        private readonly OracleOdbcService _oracleService;

        public PaDigitalController(OracleOdbcService oracleService)
        {
            _oracleService = oracleService;
        }

        // POST: PaDigitalController
        [HttpPost]
        public ActionResult Index()
        {
            this.ErrorLog = string.Empty;

            DataTable dtRetorno = ExecuterQuery();

            if (dtRetorno.Rows.Count == 0)
                return StatusCode(500, this.ErrorLog);

            string RecebeJason = GetJason();
            string RecebeBase64 = RetornoBase64(RecebeJason);
            ComunicaJason(RecebeBase64);

            return Ok("API funcionando!");
        }

        private string GetJason()
        {
            string retorno = "{\"idContrato\":961,\"chave\":\"AlSPGlkQ3RyOjk1MT4=TI\",\"datahora\":\"20250523100000\",\"xIdDoCliente\":\"1\",\"reponsavel\":\"Jean Carlo Zani\",\"proposito\":\"MOVIMENTACAO\",\"qtd_ins\":2,\"qtd_upd\":0,\"qtd_del\":0,\"dados\":[{\"tipo\":\"1\",\"codigotit\":\"00240012073780005\",\"codigodep\":\"\",\"nm_compl_benef\":\"LUIZ ANTONIO MATHEUS\",\"nm_social\":\"LUIZ ANTONIO MATHEUS\",\"ds_lograd\":\"RUA TESTE\",\"nr_lograd\":\"01\",\"compl_lograd\":\"FUNDOS\",\"ds_bairro\":\"CENTRO\",\"munic\":\"BOTUCATU\",\"nr_cep\":\"18600000\",\"cd_estado\":\"SP\",\"nr_ddd\":\"14\",\"nr_fone\":\"999999999\",\"end_email\":\"teste@teste.com\",\"dt_nascime\":\"19660512\",\"sexo\":\"M\",\"cd_cpf\":\"07203495833\",\"cd_ident\":\"\",\"tp_acao\":\"1\",\"dt_base\":\"20250523\",\"cd_cns\":\"700004770516503\",\"observacao\":\"OBS TESTE\"},{\"tipo\":\"1\",\"codigotit\":\"00240012073792003\",\"codigodep\":\"\",\"nm_compl_benef\":\"ERICK BIANCHINI GIMENEZ\",\"nm_social\":\"ERICK BIANCHINI GIMENEZ\",\"ds_lograd\":\"RUA TESTE\",\"nr_lograd\":\"02\",\"compl_lograd\":\"FUNDOS\",\"ds_bairro\":\"CENTRO\",\"munic\":\"BOTUCATU\",\"nr_cep\":\"18600000\",\"cd_estado\":\"SP\",\"nr_ddd\":\"14\",\"nr_fone\":\"999999999\",\"end_email\":\"teste@teste.com\",\"dt_nascime\":\"19971209\",\"sexo\":\"M\",\"cd_cpf\":\"42330507801\",\"cd_ident\":\"\",\"tp_acao\":\"1\",\"dt_base\":\"20250523\",\"cd_cns\":\"702009871478985\",\"observacao\":\"OBS TESTE 2\"}]}\r\n";
            return retorno;
        }

        private string RetornoBase64(string recebeBase64)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(recebeBase64);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private void ComunicaJason(string recebeBase64)
        {
            var chave = "AlSPGlkQ3RyOjk1MT4=TI";
            var options = new RestClientOptions("http://aliasti.duckdns.org:6216")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("//sigaph25/api_mmed.rule?sys=SIG&alvo=150&tkn=AlSPGlkQ3RyOjk1MT4%3DTI", Method.Post);
            request.AddHeader("Content-Type", "text/plain");
            var body = chave + recebeBase64;
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            RestResponse response = client.ExecuteAsync(request).GetAwaiter().GetResult();
            Console.WriteLine(response.Content);
        }

        private DataTable ExecuterQuery()
        {
            DataTable dtRetorno = new();

            string strQry = @"
                SELECT * FROM DADOSADV.AA1010
            ";

            try
            {
                dtRetorno = _oracleService.Execute(strQry);
            }
            catch (Exception ex)
            {
                this.ErrorLog = ($"Erro ao conectar no banco de dados: {ex.Message}");
            }

            return dtRetorno;
        }
    }
}
