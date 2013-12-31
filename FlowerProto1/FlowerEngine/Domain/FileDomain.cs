using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Transactions;

using NUnit.Framework;
using FlowerEngine;

namespace FlowerEngine.Domain
{
    [TestFixture]
    partial class FileDomain
    {
        private string folder {get; set;}

        public FileDomain()
        {
             this.folder = @".\" ;
        }
        public FileDomain(string folder)
        {
            this.folder = folder;           
        }

        #region public methods
        public void Regist(string filepath)
         {
            // チェック
            {
                if (System.IO.File.Exists(filepath) == false)
                {
                    // ファイルの存在
                    throw new Exception("指定されたファイルは存在しません。");
                }
            }
            //var filename = Path.GetFileName(filepath);

            try
            {
                // TODO
                //
                // 採番→ファイル複写→DB登録と本当はしたい
                // で失敗時にはゴミファイルが出来る形がいい。
                //
                var newId = getId();
                storeFile(filepath, newId);
                using (var tran = new TransactionScope())
                {
                    Regist2DB(Path.GetFileName(filepath), newId);
                    tran.Complete();
                }
            }
            catch
            {
                // ロールバック
                //  do nothing

                // 管理ファイルの削除（できれば）
            }
            finally
            {
            }
             return;
         }
        #endregion


        #region private methods

        private string getId()
        {
            string latestId = "";
            string newId = "";

            using (var entities = new Model.FlowerEntities())
            {
                latestId = (from e in entities.Files.AsEnumerable() select e.ID).Max();

                if (entities.Files.Count() == 0)
                {
                    return "1";
                }
            }
            // Next ID
            newId = (decimal.Parse(latestId) + 1).ToString();
            return newId;
        }


        private void storeFile(string filepath, string id)
        {
            System.IO.File.Copy(filepath, this.folder + id + ".dat");
            return;
        }


        private Model.File Regist2DB(string fileName, string id)
        {
            var fullpath = this.folder + fileName;
            var file = new Model.File();

            try
            {
                using (var entities = new Model.FlowerEntities())
                {
                    file.ID         = id;
                    file.Name       = fileName;
                    file.CreateUser = "dmy";
                    file.CreateDate = DateTime.Now;

                    entities.Files.Add(file);
                    entities.SaveChanges();
                }
            }
            catch
            {
                file = null;
            }
            return file ;
        }
        #endregion

    }
}
