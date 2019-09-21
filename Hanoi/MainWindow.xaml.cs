using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Hanoi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string g;
        Grid[] discs = new Grid[12];
        private int _numberOfDiscs = 12;
        private int _movesCounter = 0;
        public List<string> InstructionList = new List<string>();
        private string _currentInstruction="";
        private int _delay = 500;

        public string currentInstruction
        {
            get => _currentInstruction;
            set
            {
                if (_currentInstruction != value)
                {
                    _currentInstruction = value;
                    OnPropertyChanged();
                }
            }
        }
        public int numberOfDiscs
        {
            get => _numberOfDiscs;
            set
            {
                if (_numberOfDiscs != value)
                {
                    _numberOfDiscs = value;
                    OnPropertyChanged();
                }
            }
        }
        public int delay
        {
            get => _delay;
            set
            {
                if (_delay != value)
                {
                    _delay = value;
                    OnPropertyChanged();
                }
            }
        }
        public int movesCounter
        {
            get => _movesCounter;
            set
            {
                if (_movesCounter != value)
                {
                    _movesCounter = value;
                    OnPropertyChanged();
                }
            }
        }
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            for (int i = 0; i < _numberOfDiscs; i++)
            {
                discs[i] = (Grid)FirstPeg.Children[i];
            }
            //LbInstructions.ItemsSource = InstructionList;
        }
        private void LoadTowers()
        {
            foreach (DockPanel panel in Grid1.Children.OfType<DockPanel>())
            {
                panel.Children.Clear();
            }

            if (_numberOfDiscs > 12) return;
            for (int i = 0; i < 12; i++)
            {
                FirstPeg.Children.Add(discs[i]);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }

        private ManualResetEvent resetEvent = new ManualResetEvent(true); //pause and resume event

        Thread solverThread = null;
        private void BtnSolve_OnClick(object sender, RoutedEventArgs e)
        {
            solverThread = new Thread(() => MoveDisc(_numberOfDiscs, FirstPeg, EndPeg, MiddlePeg));
            Instruction(_numberOfDiscs,FirstPeg.Name,EndPeg.Name,MiddlePeg.Name);
            //LbInstructions.ItemsSource = InstructionList;
            //LbInstructions.Items.Refresh();
            solverThread.Start();
        }
        private void BtnPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (solverThread != null) //para dili magpataka'g reset sa event miskig wala nag start
                this.resetEvent.Reset();
        }

        private void BtnResume_OnClick(object sender, RoutedEventArgs e)
        {
            if (solverThread != null) this.resetEvent.Set();
        }

        //tower of hanoi algorithm
        private void MoveDisc(int n, DockPanel @from, DockPanel to, DockPanel aux)
        {
            resetEvent.WaitOne();
            if (n > 0)
            {
                MoveDisc(n - 1, @from, aux, to);
                //currentInstruction = $"Move disk {n} from {@from} to {to}";
                this.Dispatcher.Invoke(() =>
                {
                    movesCounter++;
                    var moveDisc = from.Children[from.Children.Count - 1];
                    from.Children.Remove(moveDisc);
                    to.Children.Add(moveDisc);
                    //from.Children.Remove(moveDisc); di pwede mauna ang add sa remove, need pa matanggal
                });
                System.Threading.Thread.Sleep((delay));
                MoveDisc(n - 1, aux, to, @from);
            }
        }

        private int lbcount = 1;
        private void Instruction(int n, string from, string to, string aux)
        {
            if (n > 0)
            {
                Instruction(n-1,from,aux,to);
                //currentInstruction = $"Move disk {n} from {from} to {to}";
                LbInstructions.Items.Add($"{lbcount}.) Move disk {n} from {from} to {to}");
                lbcount++;
                Instruction(n-1,aux,to,from);
            }
        }
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            solverThread?.Abort();
            solverThread = null; //ireset ang solver thread value
            resetEvent = new ManualResetEvent(true);
            LoadTowers();
            movesCounter = 0;
            _numberOfDiscs = 12;
            removedPegs = new Stack<UIElement>();
            LbInstructions.Items.Clear();
            lbcount = 1;

        }

        private void BtnDecreaseDisks_Onclick(object sender, RoutedEventArgs e)
        {
            //if (_numberOfDiscs <= 0) return;
            //solverThread?.Abort();
            //solverThread = null;
            //resetEvent = new ManualResetEvent(true);
            //InstructionList = new List<string>();
            //_numberOfDiscs--;
            //LoadTowers();
            //movesCounter = 0;
            if (solverThread != null) return;
            if (FirstPeg.Children.Count <= 0) return;
            removedPegs.Push(FirstPeg.Children[0]);
            FirstPeg.Children.RemoveAt(0);
            _numberOfDiscs--;


        }
        Stack<UIElement> removedPegs = new Stack<UIElement>();
        private void BtnIncreaseDisks_OnClick(object sender, RoutedEventArgs e)
        {
            //if (_numberOfDiscs > 12) return;
            //solverThread?.Abort();
            //solverThread = null;
            //resetEvent = new ManualResetEvent(true);
            //InstructionList = new List<string>();
            //_numberOfDiscs++;
            //LoadTowers();
            //movesCounter = 0;
            //FirstPeg.Children.;
            if (solverThread != null) return;
            if (removedPegs.Count <= 0) return;
            FirstPeg.Children.Insert(0,removedPegs.Pop());
            _numberOfDiscs--;
        }

        private void BtnNext_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
