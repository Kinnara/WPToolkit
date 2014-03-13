using Microsoft.Phone.Controls;
using PhoneToolkitSample.Data;
using System.Windows;

namespace PhoneToolkitSample.Samples
{
    public partial class ReaderboardEffectSample2 : BasePage
    {
        private bool _isDataLoaded;

        private MultilineItemCollection viewModel = new MultilineItemCollection();

        public ReaderboardEffectSample2()
        {
            InitializeComponent();

            DataContext = viewModel;
            this.Loaded += MainPage_Loaded;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isDataLoaded)
            {
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here one", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here two", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here three", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here four", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here five", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here six", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here seven", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here eight", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here nine", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here ten", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here eleven", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twelve", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here thirteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here fourteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here fifteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here sixteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here seventeen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here eighteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here nineteen", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-one", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-two", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-three", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-four", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-five", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-six", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-seven", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                viewModel.Items.Add(new MultilineItem() { LineOne = "Tap here twenty-eight", LineTwo = "Go back using transitions", LineThree = "This effect animates items individually" });
                _isDataLoaded = true;
            }
        }

        // Initiate a backward navigation.
        private void Item_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}