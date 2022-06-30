using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TrackerHabitos
{
    class Program
    {
        static string conexaoBD = @"Data Source=tracker-habitos.db";

        static void Main(string[] args)
        {
            using (var conexao = new SqliteConnection(conexaoBD))
            {
               
                conexao.Open();
                var tabela = conexao.CreateCommand();


                tabela.CommandText =
                    @"CREATE TABLE IF NOT EXISTS beber_agua (
                      Id INTEGER PRIMARY KEY AUTOINCREMENT,
                      Data TEXT,
                      Quantidade INTEGER
                     )";

                tabela.ExecuteNonQuery();

                conexao.Close();

                CapturaEntrada();
            }
        }

        static void CapturaEntrada()
        {
            Console.Clear();

            bool fecharApp = false;

            while(fecharApp == false)
            {
                Console.WriteLine("MENU PRINCIPAL\n");
                Console.WriteLine("Escolha uma opção:\n");
                Console.WriteLine("0 - Fechar o aplicativo");
                Console.WriteLine("1 - Exibir os registros");
                Console.WriteLine("2 - Adicionar registro");
                Console.WriteLine("3 - Deletar registro");
                Console.WriteLine("4 - Atualizar registro");
                Console.WriteLine("---------------------------------------------");


                string entrada = Console.ReadLine();

                switch (entrada)
                {
                    case "0":
                        Console.WriteLine("\nObrigado por utilizar!\n");
                        fecharApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        RetornaRegistros();
                        break;
                    case "2":
                        InsereRegistros();
                        break;
                    case "3":
                        DeletaRegistros();
                        break;
                    case "4":
                        AtualizaRegistros();
                        break;
                    default:
                        Console.WriteLine("\nOpção Inválida! Digite um valor entre 0 e 4.\n");
                        break;
                }
            }

        }

        private static void InsereRegistros()
        {
            string data = CapturaData();
            int quantidade = CapturaQuantidade("Insira a quantidade de copos d'água (não é permitido números decimais):");

            using (var conexao = new SqliteConnection(conexaoBD))
            {

                conexao.Open();
                var tabela = conexao.CreateCommand();


                tabela.CommandText =
                    $"INSERT INTO beber_agua(Data, Quantidade) VALUES ('{data}',{quantidade})";

                tabela.ExecuteNonQuery();

                conexao.Close();
            }

            Console.Clear();
            Console.WriteLine("Registro Inserido com sucesso!\n");
        }

        internal static string CapturaData()
        {
            Console.WriteLine("\nInsira uma data: (dd-mm-yy). Digite 0 para retornar para o menu principal.");

            string capturaData = Console.ReadLine();

            if (capturaData == "0")
            {
                CapturaEntrada();
            }

            while(!DateTime.TryParseExact(capturaData, "dd-MM-yy", new CultureInfo("pt-BR"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("Data inválida. Digite 0 para retornar ao menu principal ou tente novamente: \n");
                capturaData = Console.ReadLine();
            }

            return capturaData;
        }

        internal static int CapturaQuantidade(string aviso)
        {
            Console.WriteLine(aviso);

            string valor = Console.ReadLine();


            if (valor == "0")
            {
                CapturaEntrada();
            }

            while (!Int32.TryParse(valor, out _) || Convert.ToInt32(valor) < 0)
            {
                Console.WriteLine("Valor inválido. Digite 0 para retornar ao menu principal ou tente novamente: \n");
                valor = Console.ReadLine();
            }

            int valorFinal = Convert.ToInt32(valor);

            return valorFinal;
            
        }

        public class BeberAgua
        {
            public int Id { get; set; }
            public DateTime Data { get; set; }
            public int Quantidade { get; set; }
        }

        private static void RetornaRegistros()
        {
            Console.Clear();

            using (var conexao = new SqliteConnection(conexaoBD))
            {

                conexao.Open();
                var tabela = conexao.CreateCommand();


                tabela.CommandText =
                    $"SELECT * FROM beber_Agua";

                List<BeberAgua> dadosTabela = new();

                SqliteDataReader leitor = tabela.ExecuteReader();

                if (leitor.HasRows)
                {
                    while (leitor.Read())
                    {
                        dadosTabela.Add(
                            new BeberAgua
                            {
                                Id = leitor.GetInt32(0),
                                Data = DateTime.ParseExact(leitor.GetString(1), "dd-MM-yy", new CultureInfo("pt-BR")),
                                Quantidade = leitor.GetInt32(2),
                            }); ;//
                    }
                }
                else
                {
                    Console.WriteLine("Tabela vazia!");
                }

                conexao.Close();

                Console.WriteLine("---------------------------------------------\n");

                foreach(var bebeAgua in dadosTabela)
                {
                    Console.WriteLine($"{bebeAgua.Id} - {bebeAgua.Data.ToString("dd-MM-yyyy")} - Quantidade: {bebeAgua.Quantidade}");
                }

                Console.WriteLine("---------------------------------------------\n");
            }

        }

        private static void DeletaRegistros()
        {
            Console.Clear();
            RetornaRegistros();

            var idRegistro = CapturaQuantidade("Insira o ID do registro que deseja deletar ou digite 0 para retornar ao menu principal");

            using (var conexao = new SqliteConnection(conexaoBD))
            {

                conexao.Open();
                var tabela = conexao.CreateCommand();


                tabela.CommandText =
                    $"DELETE from beber_agua WHERE Id = '{idRegistro}'";

                int quantidadeLinhas = tabela.ExecuteNonQuery();

                if (quantidadeLinhas == 0)
                {
                    Console.WriteLine($"O Registro de ID {idRegistro} não existe!");
                    DeletaRegistros();
                }
                else
                {
                    Console.WriteLine($"O Registro de ID {idRegistro} foi deletado com sucesso!");
                }

                conexao.Close();
            }

            

        }

        internal static void AtualizaRegistros()
        {
            Console.Clear();
            RetornaRegistros();

            var idRegistro = CapturaQuantidade("Insira o ID do registro que deseja alterar ou digite 0 para retornar ao menu principal!");

            using (var conexao = new SqliteConnection(conexaoBD))
            {

                conexao.Open();

                var checaTabela = conexao.CreateCommand();
                checaTabela.CommandText = $"SELECT EXISTS(SELECT 1 FROM beber_agua WHERE Id = {idRegistro})";
                int checaRetorno = Convert.ToInt32(checaTabela.ExecuteScalar());

                if (checaRetorno == 0)
                {
                    Console.WriteLine($"O registro de ID {idRegistro} não existe.");
                    conexao.Close();
                    AtualizaRegistros();
                }

                string novaData = CapturaData();
                int novaQuantidade = CapturaQuantidade("Insira a quantidade de água atualizada. Não é permitido números decimais!");

                var tabela = conexao.CreateCommand();


                tabela.CommandText = $"UPDATE beber_agua SET Data = '{novaData}', Quantidade = {novaQuantidade} WHERE Id = {idRegistro}";
                    

                tabela.ExecuteNonQuery();

                conexao.Close();

                
            }

        }
    }
}