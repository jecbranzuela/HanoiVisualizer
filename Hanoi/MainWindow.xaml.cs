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
        public Queue<string> InstructionList = new Queue<string>();
        private string _currentInstruction=""; // wa pani nagamit
        private int _delay = 500;
        Thread solverThread = null;
        private ManualResetEvent resetEvent = new ManualResetEvent(true); //pause and resume event
        private int lbcount = 1;
        private Queue<int> diskToMove = new Queue<int>();
        private Queue<string> pegFrom = new Queue<string>();
        private Queue<string> pegTo = new Queue<string>();
        private Stack<int> diskmoved = new Stack<int>();
        private Stack<string> previouspegfrom = new Stack<string>();
        private Stack<string> previouspegto = new Stack<string>();
        private Stack<UIElement> diskContainer = new Stack<UIElement>();
        private Stack<int> diskFromPrev = new Stack<int>();
        private Stack<string> pegfromFromPrev = new Stack<string>();
        private Stack<string> pegtoFromPrev = new Stack<string>();
        public string currentInstruction
        {
            get => _currentInstruction;
            set
            {
                if (_currentInstruction == value) return;
                _currentInstruction = value;
                OnPropertyChanged();
            }
        }
        public int numberOfDiscs
        {
            get => _numberOfDiscs;
            set
            {
                if (_numberOfDiscs == value) return;
                _numberOfDiscs = value;
                OnPropertyChanged();
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
                if (_movesCounter == value) return;
                _movesCounter = value;
                OnPropertyChanged();
            }
        }
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            for (int i = 0; i < _numberOfDiscs; i++) discs[i] = (Grid)FirstPeg.Children[i];
        }
        private void LoadTowers()
        {
            foreach (DockPanel panel in Grid1.Children.OfType<DockPanel>()) panel.Children.Clear();
            if (_numberOfDiscs > 12) return;
            for (int i = 0; i < 12; i++) FirstPeg.Children.Add(discs[i]);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
        }
        private void BtnSolve_OnClick(object sender, RoutedEventArgs e)
        {
            solverThread = new Thread(() => MoveDisc(_numberOfDiscs, FirstPeg, EndPeg, MiddlePeg));
            Instruction(_numberOfDiscs,FirstPeg.Name,EndPeg.Name,MiddlePeg.Name);
            solverThread.Start();
            BtnSolve.IsEnabled = false;
            btnManual.IsEnabled = false;
            BtnPrevious.IsEnabled = false;
            BtnNext.IsEnabled = false;
            BtnPause.IsEnabled = true;
            BtnResume.IsEnabled = true;
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
        private void MoveDisc(int n, DockPanel @from, DockPanel to, DockPanel aux)
        {
            resetEvent.WaitOne();
            if (n > 0)
            {
                MoveDisc(n - 1, @from, aux, to);
                this.Dispatcher.Invoke(() =>
                {
                    movesCounter++;
                    var moveDisc = from.Children[from.Children.Count - 1];
                    from.Children.Remove(moveDisc);
                    to.Children.Add(moveDisc);
                });
                System.Threading.Thread.Sleep((delay));
                MoveDisc(n - 1, aux, to, @from);
            }
        }
        private void Instruction(int n, string from, string to, string aux)
        {
            if (n <= 0) return;
            Instruction(n-1,@from,aux,to);
            InstructionList.Enqueue($"{lbcount}.) Move disk {n} from {@from} to {to}");
            diskToMove.Enqueue(n);
            pegFrom.Enqueue(@from);
            pegTo.Enqueue(to);
            LbInstructions.Items.Add($"{lbcount}.) Move disk {n} from {@from} to {to}");
            lbcount++;
            Instruction(n-1,aux,to,@from);
        }
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            solverThread?.Abort();
            LoadTowers();
            LbInstructions.Items.Clear();
            BtnNext.IsEnabled = false;
            BtnPrevious.IsEnabled = false;
            BtnSolve.IsEnabled = true;
            BtnPause.IsEnabled = false;
            BtnResume.IsEnabled = false;
            btnManual.IsEnabled = true;
            BtnIncreaseDisks.IsEnabled = true;
            BtnDecreaseDisks.IsEnabled = true;
            movesCounter = 0; //instantiate fields used anew, para mura na'g virgin
            _numberOfDiscs = 12;
            currentInstruction = "";
            lbcount = 1;
            delay = 500;
            solverThread = null;
            resetEvent = new ManualResetEvent(true);
            InstructionList = new Queue<string>();
            diskToMove = new Queue<int>();
            pegFrom = new Queue<string>();
            pegTo = new Queue<string>();
            diskFromPrev = new Stack<int>();
            pegfromFromPrev = new Stack<string>();
            pegtoFromPrev = new Stack<string>();
            diskmoved = new Stack<int>();
            previouspegfrom = new Stack<string>();
            previouspegto = new Stack<string>();
            diskContainer = new Stack<UIElement>();
        }
        private void BtnDecreaseDisks_Onclick(object sender, RoutedEventArgs e)
        {
            if (solverThread != null) return;
            if (FirstPeg.Children.Count <= 0) return;
            diskContainer.Push(FirstPeg.Children[0]);
            FirstPeg.Children.RemoveAt(0);
            _numberOfDiscs--;
        }
        private void BtnIncreaseDisks_OnClick(object sender, RoutedEventArgs e)
        {
            if (solverThread != null) return;
            if (diskContainer.Count <= 0) return;
            FirstPeg.Children.Insert(0,diskContainer.Pop());
            _numberOfDiscs++;
        }
        private void StepMove(int diskToMove, string pegFrom, string pegTo)
        {
            UIElement disktomove = new UIElement();
            switch (diskToMove)
            {
                case 1: disktomove = disc1;
                    break;
                case 2:
                    disktomove = disc2;
                    break;
                case 3:
                    disktomove = disc3;
                    break;
                case 4:
                    disktomove = disc4;
                    break;
                case 5:
                    disktomove = disc5;
                    break;
                case 6:
                    disktomove = disc6;
                    break;
                case 7:
                    disktomove = disc7;
                    break;
                case 8:
                    disktomove = disc8;
                    break;
                case 9:
                    disktomove = disc9;
                    break;
                case 10:
                    disktomove = disc10;
                    break;
                case 11:
                    disktomove = disc11;
                    break;
                case 12:
                    disktomove = disc12;
                    break;
            }

            DockPanel pefrom = new DockPanel();
            switch (pegFrom)
            {
                case "FirstPeg": pefrom = FirstPeg;
                    break;
                case "MiddlePeg": pefrom = MiddlePeg;
                    break;
                case "EndPeg": pefrom = EndPeg;
                    break;
            }

            DockPanel pegto = new DockPanel();
            switch (pegTo)
            {
                case "FirstPeg":
                    pegto = FirstPeg;
                    break;
                case "MiddlePeg":
                    pegto = MiddlePeg;
                    break;
                case "EndPeg":
                    pegto = EndPeg;
                    break;
            }
            //var moveDisc = pefrom.Children[pefrom.Children.Count - 1];
            if (pefrom.Children.Count == 0) return;
                pefrom.Children.Remove(disktomove);
            pegto.Children.Add(disktomove);
            }
        private void PreviousStepMove(int diskToMove, string pegFrom, string pegTo)
        {
            var diskmoved = new UIElement();
            switch (diskToMove)
            {
                case 1:
                    diskmoved = disc1;
                    break;
                case 2:
                    diskmoved = disc2;
                    break;
                case 3:
                    diskmoved = disc3;
                    break;
                case 4:
                    diskmoved = disc4;
                    break;
                case 5:
                    diskmoved = disc5;
                    break;
                case 6:
                    diskmoved = disc6;
                    break;
                case 7:
                    diskmoved = disc7;
                    break;
                case 8:
                    diskmoved = disc8;
                    break;
                case 9:
                    diskmoved = disc9;
                    break;
                case 10:
                    diskmoved = disc10;
                    break;
                case 11:
                    diskmoved = disc11;
                    break;
                case 12:
                    diskmoved = disc12;
                    break;
            }

            var previouspegfrom = new DockPanel();
            switch (pegFrom)
            {
                case "FirstPeg":
                    previouspegfrom = FirstPeg;
                    break;
                case "MiddlePeg":
                    previouspegfrom = MiddlePeg;
                    break;
                case "EndPeg":
                    previouspegfrom = EndPeg;
                    break;
            }

            var previouspegto = new DockPanel();
            switch (pegTo)
            {
                case "FirstPeg":
                    previouspegto = FirstPeg;
                    break;
                case "MiddlePeg":
                    previouspegto = MiddlePeg;
                    break;
                case "EndPeg":
                    previouspegto = EndPeg;
                    break;
            }
            //var moveDisc = pefrom.Children[pefrom.Children.Count - 1];
            if (previouspegto.Children.Count == 0) return;
                previouspegto.Children.Remove(diskmoved);
            previouspegfrom.Children.Add(diskmoved);

        }
        private void BtnNext_OnClick(object sender, RoutedEventArgs e)
        {
            if (diskFromPrev.Count() != 0)
            {
                diskmoved.Push(diskFromPrev.Peek()); //gibutang ang mga gipang lihok sa stack
                previouspegfrom.Push(pegfromFromPrev.Peek()); //para ma access kung mag previous
                previouspegto.Push(pegtoFromPrev.Peek());
                StepMove(diskFromPrev.Pop(),pegfromFromPrev.Pop(),pegtoFromPrev.Pop());
                movesCounter++;
            }
            else
            {
                if (diskToMove.Count() != 0)
                {
                    diskmoved.Push(diskToMove.Peek()); //gibutang ang mga gipang lihok sa stack
                previouspegfrom.Push(pegFrom.Peek()); //para ma access kung mag previous
                previouspegto.Push(pegTo.Peek());
                StepMove(diskToMove.Dequeue(), pegFrom.Dequeue(), pegTo.Dequeue());
                movesCounter++;
                }
            }
            BtnIncreaseDisks.IsEnabled = false;
            BtnDecreaseDisks.IsEnabled = false;
            BtnSolve.IsEnabled = false;
            BtnPause.IsEnabled = false;
            BtnResume.IsEnabled = false;
            BtnPrevious.IsEnabled = true;
        }
        private void BtnPrevious_OnClick(object sender, RoutedEventArgs e)
        {
            if (diskmoved.Count() == 0) return;
            diskFromPrev.Push(diskmoved.Peek());
            pegfromFromPrev.Push(previouspegfrom.Peek());
            pegtoFromPrev.Push(previouspegto.Peek());
            PreviousStepMove(diskmoved.Pop(),previouspegfrom.Pop(),previouspegto.Pop());
            movesCounter--;
        }
        private void BtnManual_OnClick(object sender, RoutedEventArgs e)
        {
            Instruction(_numberOfDiscs, FirstPeg.Name, EndPeg.Name, MiddlePeg.Name);
            BtnNext.IsEnabled = true;
            btnManual.IsEnabled = false;
            BtnSolve.IsEnabled = false;
            BtnPause.IsEnabled = false;
            BtnResume.IsEnabled = false;
        }
    }
}
