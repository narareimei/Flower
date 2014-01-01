using System;
using System.Linq;
using System.IO;
using System.Transactions;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;

using NUnit.Framework;

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
        public string Regist(string filepath)
         {
             string newId = "";

            // チェック
            {
                if (System.IO.File.Exists(filepath) == false)
                {
                    // ファイルの存在
                    throw new Exception("指定されたファイルは存在しません。");
                }
            }

            try
            {
                //
                // 採番→ファイル複写→DB登録と本当はしたい
                // で失敗時にはゴミファイルが出来る形がいい。
                //
                newId = getId();
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
            return newId;
        }

        public Model.File Remove(string id)
        {
            if (id == null)
            {
                throw new Exception("Ivalid ID");
            }

            Model.File removedFile = null;
            try
            {
                var conn = CreateDbConnection();
                using (var tran = new TransactionScope())
                {
                    removedFile = removeFromDB(id, conn);
                    tran.Complete();
                }
                if (removedFile != null)
                {
                    removeFile(removedFile.ID);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
            }
            return removedFile;
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

        /// <summary>
        /// ファイル情報のDB登録
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>トランザクションは管理しない</remarks>
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

        private void removeFile(string id)
        {
            System.IO.File.Delete(this.folder + id + ".dat");
            return;
        }

        /// <summary>
        /// DBからのファイル情報削除
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>トランザクションは管理しない</remarks>
        private Model.File removeFromDB(string id, DbConnection conn)
        {
            Model.File removedFile = null;


            using (var entities = new Model.FlowerEntities())
            {
                var target = (from e in entities.Files.AsEnumerable() where e.ID == id select e);

                if(target != null && target.Count() > 0) {
                    removedFile = entities.Files.Remove(target.ElementAt(0));
                    entities.SaveChanges();
                }
            }
            return removedFile;
        }


        // Retrieve a connection string by specifying the providerName.
        // Assumes one connection string per provider in the config file.
        static string GetConnectionStringByProvider(string providerName)
        {
            // Return null on failure.
            string returnValue = null;

            // Get the collection of connection strings.
            ConnectionStringSettingsCollection settings =
                ConfigurationManager.ConnectionStrings;

            // Walk through the collection and return the first 
            // connection string matching the providerName.
            if (settings != null)
            {
                foreach (ConnectionStringSettings cs in settings)
                {
                    if (cs.ProviderName == providerName)
                    {
                        returnValue = cs.ConnectionString;
                        break;
                    }
                }
            }
            return returnValue;
        }


        // Given a provider name and connection string, 
        // create the DbProviderFactory and DbConnection.
        // Returns a DbConnection on success; null on failure.
        static DbConnection CreateDbConnection()
        {
            // Assume failure.
            DbConnection connection = null;

            // Create the DbProviderFactory and DbConnection.
            string entityConnectionString = ConfigurationManager.ConnectionStrings["FlowerEntities"].ConnectionString;
            string connectionString = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;
            string providerName = new EntityConnectionStringBuilder(entityConnectionString).Provider;

            if (connectionString != null)
            {
                try
                {
                    DbProviderFactory factory =
                        DbProviderFactories.GetFactory(providerName);


                    //connectionString = @"data source=|DataDirectory|\DataBase\Flower.sdf";
                    connection = factory.CreateConnection();
                    connection.ConnectionString = connectionString;
                }
                catch (Exception ex)
                {
                    // Set the connection to null if it was created.
                    if (connection != null)
                    {
                        connection = null;
                    }
                    Console.WriteLine(ex.Message);
                }
            }
            // Return the connection.
            return connection;
        }

        // This example assumes a reference to System.Data.Common.
        static DataTable GetProviderFactoryClasses()
        {
            // Retrieve the installed providers and factories.
            DataTable table = DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    Console.WriteLine(row[column]);
                }
            }
            return table;
        }

        #endregion

    }
}
