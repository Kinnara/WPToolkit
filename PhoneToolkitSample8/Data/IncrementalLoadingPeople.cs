using Microsoft.Phone.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PhoneToolkitSample.Data
{
    public class IncrementalLoadingPeople : ObservableCollection<Person>, ISupportIncrementalLoading
    {
        private IList<Person> _allPeople = AllPeople.Current.ToList();
        private bool _isInitialLoadCompleted;

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public bool HasMoreItems
        {
            get { return _isInitialLoadCompleted && Count < _allPeople.Count(); }
        }

        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (IsLoading || _isInitialLoadCompleted && !HasMoreItems || count <= 0)
            {
                return new LoadMoreItemsResult();
            }

            IsLoading = true;

            int startIndex = Count;
            uint actualCount = (uint)Math.Min(count, _allPeople.Count - Count);

            await Task.Delay(800);

            for (int i = startIndex; i < startIndex + actualCount; i++)
            {
                Add(_allPeople[i]);
            }

            _isInitialLoadCompleted = true;

            IsLoading = false;

            return new LoadMoreItemsResult { Count = actualCount };
        }
    }
}
