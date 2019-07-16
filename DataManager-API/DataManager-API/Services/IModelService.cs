using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace MLServiceAPI.Services
{
    public interface IModelService
    {
        Task<object> getValue(string name);
        Task<object> getValue(int id);
        Task<object> getValues();
        void updateValue(Object aValue);
        void createValue(Object aValue);
    }
}
