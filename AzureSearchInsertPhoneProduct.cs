// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Console.WriteLine("Início da geração do Json para inserir no índice de componentes de um telefone!");

string connectionString = "Data Source=DESKTOP-4UBQLGO;Initial Catalog=ChatOrders;User ID=sa;Password=<senha do servidor>;TrustServerCertificate=True";

try
{
    // Criar a conexão
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        // Abrir a conexão
        connection.Open();

        // Comando SQL para buscar dados necessários ao índice
        string sql = "SELECT ID, NOME FROM products";

        // Criar o comando
        using (SqlCommand command = new SqlCommand(sql, connection))
        {
            // Executar o comando e obter o resultado
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Verificar se há linhas
                if (reader.HasRows)
                {
                    List<JObject> jsonObjects = new List<JObject>();

                    // Ler os dados e imprimir na console (ajuste conforme sua necessidade)
                    while (reader.Read())
                    {
                        JObject jsonObject = new JObject();

                        // Adicionar as colunas do DataReader ao JObject
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            jsonObject.Add(reader.GetName(j), reader.GetValue(j).ToString());
                        }

                        // Adicionar o JObject à lista
                        jsonObjects.Add(jsonObject);
                    }

                    //Transforma em JArray para fazer um loop e montar cada documento do índice
                    JArray jsonArray = new JArray(jsonObjects);

                    string json = "{ \"value\":[";
                    string[] splitStore;
                    string keywords = "";
                    string nmProduct = "";
                  
                    foreach (JToken item in jsonArray)
                    {
                        if (item == null) continue;

                        nmProduct = item.SelectToken("NOME").ToString().Replace("\"", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\r\n", string.Empty);
                        splitStore = nmProduct.Split(" ");

                        foreach (var text in splitStore)
                        {
                            keywords = keywords + "\"" + text.Replace("\"", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\r\n", string.Empty) + "\",";
                        }

                        keywords = keywords.Substring(0, keywords.Length - 1);

                        json = json + "{ \"id\": \"" + item.SelectToken("ID").ToString() + "\"," + "\"@search.action\": \"upload\", \"keywords\": [ " + keywords + " ], \"name\" : \"" +
                            nmProduct + "\"},";
                        keywords = "";
                    }

                    // Serializar o JsonArray para uma string JSON
                    string jsonString = JsonConvert.SerializeObject(jsonArray);
                    Console.WriteLine("Dados a serem colados no body da requisição post para carregar o índice");
                    Console.WriteLine(json.Substring(0, json.Length - 1) + "]}");
                }
                else
                {
                    Console.WriteLine("Nenhum dado encontrado.");
                }
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Erro ao conectar ao banco de dados: " + ex.Message);
}

//post a rodar no postman
//https://search-products.search.windows.net/indexes/phone-products-search/docs/index?api-version=2017-11-11
//Api-Key:<api key do índice em questão>
//Content-Type:application/json
