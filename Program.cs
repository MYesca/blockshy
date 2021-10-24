using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace blockshy
{
    class Block
    {
        public byte[] Data {get;}
        public byte[] Hash { get; set; }
        public byte[] PrevHash { get; set; }
        public int Nonce { get; set; }
        public DateTime Timestamp {get;}

        public Block(byte[] data)
        {
            Data = data;
            Timestamp = DateTime.UtcNow;
            Nonce = 0;
            PrevHash = new byte[] {0x00};
        }

        public byte[] GenerateHash()
        {
            using(SHA512 sha = new SHA512Managed())
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(Data);
                bw.Write(Nonce);
                bw.Write(Timestamp.ToBinary());
                bw.Write(PrevHash);

                var arr = ms.ToArray();
                return sha.ComputeHash(arr);
            }
        }

        public byte[] MineHash(byte[] difficulty)
        {
            byte[] hash = GenerateHash();
            int size = difficulty.Length;

            while(!hash.Take(size).SequenceEqual(difficulty))
            {
                Nonce++;
                hash = GenerateHash();
            }

            return hash;
        }

        public bool IsValid()
        {
            return GenerateHash().SequenceEqual(Hash);
        }

        public bool IsPreviousValid(Block prevBlock)
        {
            return prevBlock.IsValid() && PrevHash.SequenceEqual(prevBlock.Hash);
        }

        public override string ToString()
        {
            return $"{BitConverter.ToString(Hash.Take(9).ToArray()).Replace("-", "")}..{BitConverter.ToString(Hash.TakeLast(9).ToArray()).Replace("-", "")}:\n   Prev: {BitConverter.ToString(PrevHash.Take(9).ToArray()).Replace("-", "")}..{BitConverter.ToString(PrevHash.TakeLast(9).ToArray()).Replace("-", "")}\n   Nonce: {Nonce} Timestamp: {Timestamp}";
        }
    }

    class Blockchain
    {
        private List<Block> _items;
        private byte[] _difficulty;

        public Blockchain(byte[] difficulty, Block genesis)
        {
            _items = new List<Block>();
            _difficulty = difficulty;            
            AddBlock(genesis);
        }

        public Block AddBlock(Block block)
        {
            block.PrevHash = _items.LastOrDefault()?.Hash ?? new byte[] { 0x00 };
            block.Hash = block.MineHash(_difficulty);

            _items.Add(block);

            return block;
        }

        public bool IsValid()
        {
            var iter = _items.GetEnumerator();

            bool isValid = iter.MoveNext() && (iter.Current?.IsValid() ?? false);

            Block previous = iter.Current;

            while(iter.MoveNext() && isValid)
            {
                isValid = iter.Current.IsPreviousValid(previous) && iter.Current.IsValid();
                previous = iter.Current;
            }

            return isValid;
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            Block genesis = new Block(new byte[] {0x00, 0x00, 0x00, 0x00, 0x00});

            Blockchain blockchain = new Blockchain(new byte[] {0x00, 0x00}, genesis);

            Console.WriteLine($"Starting! Chain brand new is valid: {blockchain.IsValid()}\n\n");

            Random rnd = new Random(DateTime.UtcNow.Millisecond);
            for(int i = 0; i < 10; i++)
            {
                byte[] data = Enumerable.Range(0, 256).Select(x => (byte)rnd.Next(256)).ToArray();
                Block block = new Block(data);

                blockchain.AddBlock(block);

                Console.WriteLine(block);
                Console.WriteLine($"Chain is valid: {blockchain.IsValid()}\n\n");
            }

            Console.WriteLine("Done!");
        }
    }
}
