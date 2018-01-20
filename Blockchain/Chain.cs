﻿using Blockchain.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using Blockchain.Exceptions;


namespace Blockchain
{
    /// <summary>
    /// Цепочка блоков.
    /// </summary>
    public class Chain
    {
        /// <summary>
        /// Алгоритм хеширования.
        /// </summary>
        private IAlgorithm _algorithm = AlgorithmHelper.GetDefaultAlgorithm();

        /// <summary>
        /// Список, содержащий в себе все блоки.
        /// </summary>
        private List<Block> _blockChain = null;

        /// <summary>
        /// Цепочка блоков.
        /// </summary>
        public IEnumerable<Block> BlockChain => _blockChain;

        /// <summary>
        /// Крайний блок в цепочке блоков.
        /// </summary>
        public Block PreviousBlock => _blockChain.Last();

        /// <summary>
        /// Блоки, содержащие информационные данные.
        /// </summary>
        public IEnumerable<Block> ContentBlocks => _blockChain.Where(block => block.Data.Type == DataType.Content);

        /// <summary>
        /// Блоки, содержащие информацию о пользователях.
        /// </summary>
        public IEnumerable<Block> UserBlocks => _blockChain.Where(block => block.Data.Type == DataType.User);

        /// <summary>
        /// Создать новый экземпляр цепочки блоков.
        /// </summary>
        public Chain()
        {
            var globalChain = GetGlobalChein();
            if(globalChain == null)
            {
                CreateNewBlockChain();
            }
            else
            {
                bool globalChainIsCorrect = globalChain.CheckCorrect();
                if(globalChainIsCorrect)
                {
                    _blockChain = globalChain._blockChain;
                }
                else
                {
                    CreateNewBlockChain();
                    throw new TypeLoadException("Полученная глобальная цепочка блоков является некорректной!");
                }
            }

            if(!CheckCorrect())
            {
                throw new MethodResultException(nameof(Chain));
            }
        }

        /// <summary>
        /// Создать новую пустую цепочку блоков.
        /// </summary>
        private void CreateNewBlockChain()
        {
            _blockChain = new List<Block>();
            var genesisBlock = Block.GetGenesisBlock(_algorithm);
            AddBlock(genesisBlock);
        }

        /// <summary>
        /// Проверить корректность цепочки блоков.
        /// </summary>
        /// <returns> Корректность цепочки блоков. true - цепочка блоков корректна, false - цепочка некорректна. </returns>
        public bool CheckCorrect()
        {
            foreach(var block in _blockChain)
            {
                if(!block.IsCorrect(_algorithm))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Получить глобальную цепочку блоков.
        /// </summary>
        /// <returns> Цепочка блоков. </returns>
        private Chain GetGlobalChein()
        {
            return null;
        }

        /// <summary>
        /// Добавить данные в цепочку блоков.
        /// </summary>
        /// <param name="text"> Добавляемые данные. </param>
        public void AddContent(string text)
        {
            if(string.IsNullOrEmpty(text))
            {
                throw new MethodRequiresException(nameof(text));
            }

            var data = new Data(text, DataType.Content);

            var block = new Block(PreviousBlock, data, User.GetCurrentUser(), _algorithm);

            AddBlock(block);
        }

        /// <summary>
        /// Добавить данные о пользователе в цепочку.
        /// </summary>
        /// <param name="login"> Имя пользователя. </param>
        /// <param name="password"> Пароль пользователя. </param>
        /// <param name="role"> Права доступа пользователя. </param>
        public void AddUser(string login, string password, UserRole role = UserRole.Reader)
        {
            if (string.IsNullOrEmpty(login))
            {
                throw new MethodRequiresException(nameof(login));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new MethodRequiresException(nameof(password));
            }

            if(UserBlocks.Any(b => b.Data.Content.Contains(login)))
            {
                throw new MethodRequiresException(nameof(login));
            }

            var user = new User(login, password, role);
            var data = user.GetData();
            var block = new Block(PreviousBlock, data, User.GetCurrentUser());
            AddBlock(block);
        }

        /// <summary>
        /// Добавить блок.
        /// </summary>
        /// <param name="block"> Добавляемый блок. </param>
        private void AddBlock(Block block)
        {
            if(!block.IsCorrect())
            {
                throw new MethodRequiresException(nameof(block));
            }

            _blockChain.Add(block);

            if(!CheckCorrect())
            {
                throw new MethodResultException(nameof(Chain));
            }
        }
    }
}