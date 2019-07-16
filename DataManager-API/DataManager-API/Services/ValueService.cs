using System;
using System.Threading.Tasks;

namespace MLServiceAPI.Services
{
    public class ValueService : IModelService
    {
        public ValueService()
        {
        }

        public void createValue(object aValue)
        {
            // TODO Create value
        }

        public Task<object> getValue(string name)
        {
            // TODO get value by name (my not be necessary)
            throw new NotImplementedException();
        }

        public Task<object> getValue(int id)
        {
            // TODO return value by ID (replace with GUID in future)
            throw new NotImplementedException();
        }

        public Task<object> getValues()
        {
            // TODO return all values
            throw new NotImplementedException();
        }

        public void updateValue(object aValue)
        {
            // TODO update value given a value
            throw new NotImplementedException();
        }
    }
}
