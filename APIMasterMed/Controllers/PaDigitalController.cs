using APIMasterMed.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

            string RecebeJason = GetJason(dtRetorno);
            string RecebeBase64 = RetornoBase64(RecebeJason);
            ComunicaJason(RecebeBase64);

            return Ok("API funcionando!");
        }

        private string GetJason(DataTable Tabela)
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
                SELECT * FROM (
SELECT
CASE WHEN BA1.BA1_TIPREG = '00' THEN 1 ELSE 2 END AS TIPO,
CASE
WHEN BA1.BA1_TIPREG = '00' THEN BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO
ELSE (
SELECT BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO
FROM DADOSADV.BA1010 DEP
WHERE DEP.D_E_L_E_T_ = ' ' AND DEP.BA1_DATBLO = '        '
AND DEP.BA1_CODINT = BA1.BA1_CODINT AND DEP.BA1_CODEMP = BA1.BA1_CODEMP AND DEP.BA1_MATRIC = BA1.BA1_MATRIC AND DEP.BA1_TIPREG = '00'  
AND ROWNUM = 1
)
    END AS CODIGO_TIT,
    CASE WHEN BA1.BA1_TIPREG != '00' THEN BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO ELSE ' ' END AS CODIGO_DEP,
    BA1.BA1_NOMUSR AS NOME,
    BA1.BA1_CPFUSR AS CPF,
    SUBSTR(BA1.BA1_DATNAS,7,2) ||'/'||SUBSTR(BA1.BA1_DATNAS,5,2)||'/'||SUBSTR(BA1.BA1_DATNAS,1,4) AS NASCIMENTO ,
    TRIM(BA1.BA1_EMAIL) AS EMAIL,
    BA1.BA1_ENDERE AS ENDERECO,
    BA1_NR_END AS NUMERO,
    BA1_COMEND AS COMPLEMENTO,
    BA1_BAIRRO AS BAIRRO,
    BA1_MUNICI AS MUNICIPIO,
    BA1_ESTADO AS UF,
    BA1_CEPUSR AS CEP,
    BA1_DDD AS DDD,
    BA1_TELEFO AS FONE,
    CASE WHEN BA1.BA1_SEXO = '2' THEN 'F' ELSE 'M' END AS SEXO,
    TO_CHAR(BA1.BA1_NRCRNA) AS CNS,
    0 AS CD_ECIVIL,
    'Unimed de Botucatu Cooperativa de Trabalho Médico' AS PLAN_ID,
    ' ' AS PLAN_STATUS,
    'AlSPGlkQ3RyOjk1MT4=TI' AS TOKEN,
    '961' AS IDCONTRATO,
    '132' AS ALVO

    FROM DADOSADV.BA1010 BA1
    INNER JOIN DADOSADV.BA3010 BA3 ON BA1_CODINT = BA3_CODINT AND BA1_CODEMP = BA3_CODEMP AND BA1_MATRIC = BA3_MATRIC    
    INNER JOIN DADOSADV.BQC010 BQC ON BA3_CODINT = BQC_CODINT AND BA3_CODEMP = BQC_CODEMP AND BA3_CONEMP = BQC_NUMCON AND BA3_VERCON = BQC_VERCON AND BA3_SUBCON = BQC_SUBCON AND BA3_VERSUB = BQC_VERSUB
    INNER JOIN DADOSADV.BT5010 BT5 ON BQC_CODINT = BT5_CODINT AND BQC_CODEMP = BT5_CODIGO AND BQC_NUMCON = BT5_NUMCON AND BQC_VERCON = BT5_VERSAO
    INNER JOIN DADOSADV.BI3010 BI3 ON BA3_CODINT = BI3_CODINT AND BA3_CODPLA = BI3_CODIGO AND BA3_VERSAO = BI3_VERSAO
    WHERE BA3.D_E_L_E_T_ = ' ' AND BA1.D_E_L_E_T_ = ' ' AND BQC.D_E_L_E_T_ = ' ' AND BT5.D_E_L_E_T_ = ' ' AND BI3.D_E_L_E_T_ = ' '
    AND BA3_FILIAL = '  ' AND BA1_FILIAL = '  ' AND BQC_FILIAL = '  ' AND BT5_FILIAL = '  ' AND BI3_FILIAL = '  '
    AND BA1_CODINT = '0024' AND BI3_CODINT = '0024' AND BA3_CODINT = '0024'
    AND
    (
      (BA1_CODEMP = '0532' AND BA1_CONEMP = '000000000001' AND BA1_SUBCON = '000000001') -- PLANO FUNCIONARIOS
      OR
      BA1_CODEMP NOT IN ('0001','0008','0049','0050','0083','0211','0228','0250','0252','0532','9002','9008','0992','9994','9970','0998','9999') -- GRUPOS QUE NÃO ENTRAM
    )
    AND BT5_TIPOIN IN ('  ','', '12')
    AND BA3_TIPOUS = '2'
    AND BA3_CODPLA NOT IN ('0042','0027','0048') -- Produto SOS Unimed e produtos Enf e Apto de sinistrados de responsabilidade da FESP
    AND (BA1_DATBLO = ' ' OR BA1_DATBLO >= TO_CHAR(SYSDATE,'YYYYMMDD') )

   
    UNION


    SELECT
CASE WHEN BA1.BA1_TIPREG = '00' THEN 1 ELSE 2 END AS TIPO,
CASE
WHEN BA1.BA1_TIPREG = '00' THEN BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO
ELSE (
SELECT BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO
FROM DADOSADV.BA1010 DEP
WHERE DEP.D_E_L_E_T_ = ' ' AND DEP.BA1_DATBLO = '        '
AND DEP.BA1_CODINT = BA1.BA1_CODINT AND DEP.BA1_CODEMP = BA1.BA1_CODEMP AND DEP.BA1_MATRIC = BA1.BA1_MATRIC AND DEP.BA1_TIPREG = '00'  
AND ROWNUM = 1
)
    END AS CODIGO_TIT,
    CASE WHEN BA1.BA1_TIPREG != '00' THEN BA1_CODINT || BA1_CODEMP || BA1_MATRIC || BA1_TIPREG || BA1_DIGITO ELSE ' ' END AS CODIGO_DEP,
    BA1.BA1_NOMUSR AS NOME,
    BA1.BA1_CPFUSR AS CPF,
    SUBSTR(BA1.BA1_DATNAS,7,2) ||'/'||SUBSTR(BA1.BA1_DATNAS,5,2)||'/'||SUBSTR(BA1.BA1_DATNAS,1,4) AS NASCIMENTO ,
    TRIM(BA1.BA1_EMAIL) AS EMAIL,
    BA1.BA1_ENDERE AS ENDERECO,
    BA1_NR_END AS NUMERO,
    BA1_COMEND AS COMPLEMENTO,
    BA1_BAIRRO AS BAIRRO,
    BA1_MUNICI AS MUNICIPIO,
    BA1_ESTADO AS UF,
    BA1_CEPUSR AS CEP,
    BA1_DDD AS DDD,
    BA1_TELEFO AS FONE,
    CASE WHEN BA1.BA1_SEXO = '2' THEN 'F' ELSE 'M' END AS SEXO,
    TO_CHAR(BA1.BA1_NRCRNA) AS CNS,
    0 AS CD_ECIVIL,
    'Unimed de Botucatu Cooperativa de Trabalho Médico' AS PLAN_ID,
    ' ' AS PLAN_STATUS,
    'AlSPGlkQ3RyOjk1MT4=TI' AS TOKEN,
    '961' AS IDCONTRATO,
    '132' AS ALVO
    FROM DADOSADV.BA1010 BA1
    INNER JOIN DADOSADV.BA3010 BA3 ON BA1_CODINT = BA3_CODINT AND BA1_CODEMP = BA3_CODEMP AND BA1_MATRIC = BA3_MATRIC
    INNER JOIN DADOSADV.BI3010 BI3 ON BA3_CODINT = BI3_CODINT AND BA3_CODPLA = BI3_CODIGO AND BA3_VERSAO = BI3_VERSAO
    WHERE BA3.D_E_L_E_T_ = ' ' AND BA1.D_E_L_E_T_ = ' ' AND BI3.D_E_L_E_T_ = ' '
    AND BA3_FILIAL = '  ' AND BA1_FILIAL = '  ' AND BI3_FILIAL = '  '
    AND BA1_CODINT = '0024' AND BI3_CODINT = '0024' AND BA3_CODINT = '0024'
    AND
    (
      (BA1_CODEMP = '0532' AND BA1_CONEMP = '000000000001' AND BA1_SUBCON = '000000001') -- PLANO FUNCIONARIOS
      OR
      BA1_CODEMP NOT IN ('0001','0008','0049','0050','0083','0211','0228','0250','0252','0532','9002','9008','0992','9994','9970','0998','9999') -- GRUPOS QUE NÃO ENTRAM
    )
    AND BA3_TIPOUS = '1'
    AND BA3_CODPLA NOT IN ('0042','0027','0048') -- Produto SOS Unimed e produtos Enf e Apto de sinistrados de responsabilidade da FESP
    AND (BA1_DATBLO = ' ' OR BA1_DATBLO >= TO_CHAR(SYSDATE,'YYYYMMDD') )

) TAB
ORDER BY CODIGO_TIT,CODIGO_DEP
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
