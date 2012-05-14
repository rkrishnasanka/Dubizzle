using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;

namespace Dubizzle
{
	public partial class MultiLevelAnimatingListBox : UserControl
	{
        public IEnumerable Level1_ItemSource { set { ListBox_Level1.ItemsSource = value; } get { return ListBox_Level1.ItemsSource; } }
        public IEnumerable Level2_ItemSource { set { ListBox_Level2.ItemsSource = value; } get { return ListBox_Level2.ItemsSource; } }
        public IEnumerable Level3_ItemSource { set { ListBox_Level3.ItemsSource = value; } get { return ListBox_Level3.ItemsSource; } }
        public IEnumerable Level4_ItemSource { set { ListBox_Level4.ItemsSource = value; } get { return ListBox_Level4.ItemsSource; } }

        public event EventHandler DoneSelectingListBox4;
        public event EventHandler DoneSelectingListBox3;
        public event EventHandler DoneSelectingListBox2;
        public event EventHandler DoneSelectingListBox1;


        public MultiLevelAnimatingListBox()
		{
			// Required to initialize variables
			InitializeComponent();
		}



		private void ListBox_Level1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            Level1Animation.Begin();
            if(this.DoneSelectingListBox1 != null)
                this.DoneSelectingListBox1(this, new EventArgs ());
		}

		private void ListBox_Level2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            Level2Animaion.Begin();
            if(this.DoneSelectingListBox2 != null)
                this.DoneSelectingListBox2(this, new EventArgs());

		}

		private void ListBox_Level4_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            if (this.DoneSelectingListBox4 != null)
                this.DoneSelectingListBox4(this, new EventArgs());

		}

		private void ListBox_Level3_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            Level3Animation.Begin();
            if(this.DoneSelectingListBox3 != null)
                this.DoneSelectingListBox3(this, new EventArgs());

		}
	}
}