   M S B A M L   `   `   `  ����  	SQLiteGui   SQLiteGui.AddTableWindow   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -   5        $	��260��5   	   $	��400��5   	     � DataContext  5	   	   ��  �� RelativeSource ��  �� Mode$
 Self=�.��5      �  � ColumnDefinitions 5   
   �� 5      $��5 �6   �� 5      $��80 �6   �� 5      $��5 �6   �� 5      $��80 �6   �� 5      $��5 �6   �� 5      $��80 �6   �� 5      $��5 �6   �� 5      $��* �6   �� 5      $��5 �6   �� 5      $��80 �6   �� 5      $��5 �6    � RowDefinitions 5   
   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5       $��25 �6   �� 5!      $��* �6   �� 5"      $��5 �6   �� 5#      $��25 �6   �� 5$      $��5 �6   .+�5&   
   �� -   5         ��Name$ TableNameLbl��5&      $��1��5'      $��Stretch=�5(      $��1��5)      $��Stretch=�5*      $��3��5+      �� 5,   
   -   5         ��Name$ TxtBoxTableName��5-      $��5��5,      $��Stretch=�5.      $��1��5/       �� TextWrapping$
 Wrap=�50      $��Stretch=�51      $��2��52      &   System.Windows.Controls.DataGrid  53   
   -   5          Name$ DataGridTbl��54      	   AutoGenerateColumns$
	 True��55      
   CanUserReorderColumns$

 True��56         CanUserResizeColumns$
 True��57         CanUserResizeRows$ False��58         CanUserSortColumns$ False��59         CanUserAddRows$
 True��5:      $��1��5;      $��Stretch=�5<      $��3��5=      $��Stretch=�5>      $��7��5?      $��4��5@   3   ��6   �� *TableElements+63   �� 5A   
   -   5         ��Name$ 
BtnExecute��5A      $��1��5B      $��Stretch=�5C      $��8��5D      $��Stretch=�5E      5F      �� 5G   
   -   5        $ 	BtnCancel��5G      $��3��5H      $��Stretch=�5I      $��8��5J      $��Stretch=�5K      5L      �� 5M   
   -   5        $ BtnAdd��5M      $��9��5N      $��Stretch=�5O      $��3��5P      $��Stretch=�5Q      5R      �� 5S   
   -   5        $ 	BtnDelete��5S      $��9��5T      $��Stretch=�5U      $��5��5V      $��Stretch=�5W      5X      5Y      5Z      