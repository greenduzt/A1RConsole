using A1RConsole.Models.Stock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1RConsole.Models.Machines
{
    public class Machine : INotifyPropertyChanged
    {
        public int MachineID { get; set; }
        public string MachineDescription { get; set; }
        public string MachineString { get; set; }
        public MachineGroup MachineGroup { get; set; }
        private string _machineName;
        private string _machineType;
        private StockLocation _stockLocation;
        private bool _isSelected;
        private double _mixes;
        private string _people;
        private double _max;
        private double _min;
        private bool _isActive;

        public string MachineName
        {
            get { return _machineName; }
            set
            {
                _machineName = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MachineName"));
                }
            }
        }



        public double Mixes
        {
            get { return _mixes; }
            set
            {
                _mixes = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Mixes"));
                }
            }
        }

        public double Max
        {
            get { return _max; }
            set
            {
                _max = value;

            }
        }

        public double Min
        {
            get { return _min; }
            set
            {
                _min = value;

            }
        }

        public string People
        {
            get { return _people; }
            set
            {
                _people = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("People"));
                }
            }
        }

        public string MachineType
        {
            get { return _machineType; }
            set
            {
                _machineType = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("MachineType"));
                }
            }
        }

        public StockLocation StockLocation
        {
            get { return _stockLocation; }
            set
            {
                _stockLocation = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("StockLocation"));
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                }
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsActive"));
                }
            }
        }

        //public Machines(double scr)
        //{
        //    this.Mixes = scr;
        //}



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

    }
}
