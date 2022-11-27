namespace JsonServerKit.DataAccess
{
        /// <summary>
        /// Interface to the repository.
        /// </summary>
        /// <typeparam name="T">Type of object to be operated on.</typeparam>
        public interface IRepository<T>
        {
            void Create(T entity);
            T Read(object id);
            void Update(T entity);
            T CreateOrUpdate(T entity);
            void Delete(object id);
            void Delete(T entity);
            void DeleteAll();
        }
}
