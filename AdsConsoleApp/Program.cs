using System;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.SumCommand;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace AdsConsoleApp
{
    internal class Program
    {
        private const string NetId = "5.62.84.145.1.1";
        private const int Port = 27906;

        private static void Main(string[] args)
        {

            ReadMultipleVariablesAsDynamic();

            using (var client = new AdsClient())
            {
                client.Connect(NetId, Port);

                ReadSingleVariable(client);

                PrintSymbols(client);
            }

            Console.ReadLine();
        }

        private static void ReadMultipleVariablesAsDynamic()
        {
            using (AdsSession session = new AdsSession(new AmsAddress(NetId, Port)))
            {
                AdsConnection connection = (AdsConnection)session.Connect();
                var symbols = session.SymbolServer.Symbols;

                ISymbol var1 = symbols["KLEMME 4 (EL3204).RTD INPUTS CHANNEL 1"];
                ISymbol var2 = symbols["KLEMME 4 (EL3204).RTD INPUTS CHANNEL 2"];
                ISymbol var3 = symbols["KLEMME 4 (EL3204).RTD INPUTS CHANNEL 1.STATUS.OVERRANGE"];

                var symbolsToRead = new SymbolCollection() { var1, var2, var3 };

                SumSymbolRead readCommand = new SumSymbolRead(connection, symbolsToRead);
                for (int i = 0; i < 20; i++)
                {
                    dynamic[] values = readCommand.Read();
                    PrintValue(0, values);
                    PrintValue(1, values);
                    Console.WriteLine(values[2]);
                    Console.WriteLine();
                }
            }

            void PrintValue(int index, dynamic[] values) 
                => Console.WriteLine($"Value {index + 1}: {values[index].VALUE}, Status {values[index].STATUS}, Overrange {values[index].STATUS.OVERRANGE}");
        }

        private static void ReadSingleVariable(AdsClient client)
        {
            var symbol = client.CreateVariableHandle("KLEMME 4 (EL3204).RTD INPUTS CHANNEL 1");

            var buffer = new Memory<byte>(new byte[4]);
            var value = client.Read(symbol, buffer);
        }

        private static void PrintSymbols(AdsClient client)
        {
            SymbolLoaderSettings settings = new SymbolLoaderSettings(TwinCAT.SymbolsLoadMode.VirtualTree);
            ISymbolLoader loader = SymbolLoaderFactory.Create(client, settings);

            var symbols = loader.Symbols;

            PrintSymbols(symbols);
        }

        private static void PrintSymbols(ISymbolCollection<ISymbol> symbols)
        {
            foreach (Symbol symbol in symbols)
            {
                object value = symbol.HasValue ? symbol.ReadValue() : "Has no value";
                Console.WriteLine($"{symbol.InstancePath}, {symbol.IndexGroup}, {symbol.IndexOffset} {value}, {symbol.TypeName}");

                if (symbol.SubSymbolCount > 0)
                    PrintSymbols(symbol.SubSymbols);
            }
        }
    }
}