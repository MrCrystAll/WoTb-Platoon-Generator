using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WotGenC.Missions
{
    public abstract class Mission : INotifyPropertyChanged
    {
        private string _intitule;
        public string Intitule
        {
            get => _intitule;
            set
            {
                _intitule = value;
                OnPropertyChanged(nameof(Intitule));
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
