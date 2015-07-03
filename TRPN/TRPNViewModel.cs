using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TRPN
{
    public class TRPNViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The stack
        /// </summary>
        private Stack<double> stack = new Stack<double>();

        /// <summary>
        /// The current working input
        /// </summary>
        private StringBuilder input = new StringBuilder();

        /// <summary>
        /// The Maximum number of rows to display
        /// </summary>
        private static int maxrows = 6;

        /// <summary>
        /// Is the display larger than the maximum number of rows
        /// </summary>
        public bool DisplayToBig
        {
            get { return stack.Count > (maxrows - 1); }
        }

        /// <summary>
        /// Get the length of the input row ignoring minus signs
        /// </summary>
        private int inputLength 
        {
            get
            {
                if (input.Length == 0)
                {
                    return 0;
                }
                else
                {
                    return input.ToString().Substring(0, 1) == "-" ? input.Length - 1 : input.Length;
                }
            }
        }

        /// <summary>
        /// Get the display
        /// </summary>
        public string StackDisplay
        {
            get 
            {
                StringBuilder display = new StringBuilder();
                
                Stack<double> stackToShow = stack.Count <= (maxrows - 1) ? stack : new Stack<double>(stack.ToList().GetRange(0, (maxrows - 1)).AsEnumerable().Reverse());

                foreach(double d in stackToShow.Reverse())
                {
                    display.AppendLine(string.Format("{0}", d));
                }
                display.AppendLine(inputLength > 0 ? input.ToString() : "0");
                return display.ToString();
            }
        }

        #region EnterButton
        private RelayCommand _EnterCommand;

        /// <summary>
        /// Command when Enter pressed
        /// </summary>
        public ICommand EnterCommand
        {
            get
            {
                if (_EnterCommand == null)
                {
                    _EnterCommand = new RelayCommand(param => this.EnterPressed(), param => this.CanEnter);
                }
                return _EnterCommand;
            }
        }

        /// <summary>
        /// Enter command can be called
        /// </summary>
        public bool CanEnter 
        { 
            get 
            {
                if (inputLength != 0)
                {
                    return true;
                }
                return false;
            } 
        }

        /// <summary>
        /// Enter command called
        /// </summary>
        private void EnterPressed()
        {
            if (inputLength != 0)
            {
                stack.Push(double.Parse(input.ToString()));
                input.Clear();
                OnPropertyChanged("stack");
                OnPropertyChanged("input");
                OnPropertyChanged("DisplayToBig");
                OnPropertyChanged("StackDisplay");
            }
        }
        #endregion

        #region NumberButton
        private RelayCommand _NumberCommand;

        /// <summary>
        /// Command when a number is entered
        /// </summary>
        public ICommand NumberCommand
        {
            get
            {
                if (_NumberCommand == null)
                {
                    _NumberCommand = new RelayCommand(param => NumberPressed((string)param));
                }
                return _NumberCommand;
            }
        }

        /// <summary>
        /// Number command called
        /// </summary>
        /// <param name="num">The number as a <see cref="System.String"/></param>
        private void NumberPressed(string num)
        {
            int number;
            if (!int.TryParse(num, out number)) { throw new ArgumentException("NumberPressed"); }
            if (number < 0 || number > 9) { throw new ArgumentOutOfRangeException("NumberPressed"); }

            if (!(num == "0" && inputLength == 0))
            {
                input.Append(num);
            }
            OnPropertyChanged("input");
            OnPropertyChanged("StackDisplay");
        }
        #endregion
        
        #region MathsButton
        private RelayCommand _MathsCommand;

        /// <summary>
        /// Command when a maths command is given that requires 2 numbers
        /// </summary>
        public ICommand MathsCommand
        {
            get
            {
                if (_MathsCommand == null)
                {
                    _MathsCommand = new RelayCommand(param => MathsPressed((string)param),param=>this.CanDoMaths);
                }
                return _MathsCommand;
            }
        }

        /// <summary>
        /// Two number maths can be executed
        /// </summary>
        private bool CanDoMaths
        {
            get
            {
                return (stack.Count >= 2 || (stack.Count == 1 && inputLength != 0));
            }
        }

        /// <summary>
        /// Two number maths command called
        /// </summary>
        /// <param name="command">Command</param>
        private void MathsPressed(string command)
        {
            if (inputLength > 0)
            {
                stack.Push(double.Parse(input.ToString()));
                input.Clear();
            }
            double out2 = stack.Pop();
            double out1 = stack.Pop();

            switch (command)
            {
                case "plus": stack.Push(out1 + out2); break;
                case "minus": stack.Push(out1 - out2); break;
                case "times": stack.Push(out1 * out2); break;
                case "divide": stack.Push(out1 / out2); break;
                case "swap": stack.Push(out2); input = new StringBuilder(out1.ToString()); break;
                default: stack.Push(out1); stack.Push(out2); break;
            }

            OnPropertyChanged("stack");
            OnPropertyChanged("input");
            OnPropertyChanged("DisplayToBig");
            OnPropertyChanged("StackDisplay");
        }
        #endregion

        #region SMathsButton
        private RelayCommand _SMathsCommand;

        /// <summary>
        /// Command when a maths command is given that only requires 1 number
        /// </summary>
        public ICommand SMathsCommand
        {
            get
            {
                if (_SMathsCommand == null)
                {
                    _SMathsCommand = new RelayCommand(param => SMathsPressed((string)param), param => this.CanDoSMaths);
                }
                return _SMathsCommand;
            }
        }

        /// <summary>
        /// Single number maths can be executed
        /// </summary>
        private bool CanDoSMaths
        {
            get
            {
                return (inputLength != 0);
            }
        }

        /// <summary>
        /// Single number maths command called
        /// </summary>
        /// <param name="command">Command</param>
        private void SMathsPressed(string command)
        {
            double dblInput = double.Parse(input.ToString());
            
            switch (command)
            {
                case "sqrt": dblInput = Math.Sqrt(dblInput); input = new StringBuilder(string.Format("{0}", dblInput)); break;
                case "exp": if (!input.ToString().Contains("E")) { input.Append("E"); } break;
                default: break;
            }

            OnPropertyChanged("stack");
            OnPropertyChanged("input");
            OnPropertyChanged("DisplayToBig");
            OnPropertyChanged("StackDisplay");
        }
        #endregion

        #region ClearCommand
        private RelayCommand _ClearCommand;

        /// <summary>
        /// Command to clear
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                if (_ClearCommand == null)
                {
                    _ClearCommand = new RelayCommand(param => ClearPressed(), param => this.CanClear);
                }
                return _ClearCommand;
            }
        }

        /// <summary>
        /// Clear command can be called
        /// </summary>
        private bool CanClear
        {
            get
            {
                return (stack.Count >= 1 || inputLength != 0);
            }
        }

        /// <summary>
        /// Clear last row
        /// </summary>
        private void ClearPressed()
        {
           if (inputLength > 0)
            {
                input.Clear();
                OnPropertyChanged("input");
                OnPropertyChanged("StackDisplay");
                return;
            }
            else
            {
                if (stack.Count > 0)
                {
                    stack.Pop();
                    OnPropertyChanged("stack");
                    OnPropertyChanged("DisplayToBig");
                    OnPropertyChanged("StackDisplay");
                    return;
                }
            }
        }
        #endregion

        #region PlusMinusCommand
        private RelayCommand _PlusMinusCommand;
        /// <summary>
        /// Plus/minus command
        /// </summary>
        public ICommand PlusMinusCommand
        {
            get
            {
                if (_PlusMinusCommand == null)
                {
                    _PlusMinusCommand = new RelayCommand(param => PlusMinusPressed(), param => CanDoPlusMinus);
                }
                return _PlusMinusCommand;
            }
        }

        /// <summary>
        /// plus/minus can be called
        /// </summary>
        private bool CanDoPlusMinus
        {
            get
            {
                return inputLength > 0;
            }
        }

        /// <summary>
        /// Change the sign of the data or exponential
        /// </summary>
        private void PlusMinusPressed()
        {
            if (input.ToString().Contains("E"))
            {
                int posOfExp = input.ToString().IndexOf("E");
                if (posOfExp != input.Length)
                {
                    if (input.ToString().Substring(posOfExp + 1, 1) == "-")
                    {
                        input.Remove(posOfExp + 1, 1);
                    }
                    else
                    {
                        input.Insert(posOfExp + 1, "-");
                    }
                }
            }
            else
            {
                if (input.ToString().Substring(0, 1) == "-")
                {
                    input.Remove(0, 1);
                }
                else
                {
                    input.Insert(0, "-");
                }
            }
            OnPropertyChanged("input");
            OnPropertyChanged("StackDisplay");
            return;
        }
        #endregion




        /* INotifyPropertyChanged */

        /// <summary>
        /// Triggered when a class property value is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper for PropertyChanged event
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
