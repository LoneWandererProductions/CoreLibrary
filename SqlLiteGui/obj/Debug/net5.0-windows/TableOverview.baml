   M S B A M L   `   `   `  ����D  @SQLiteGui, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:SQLiteGui	SQLiteGui  5
          SQLiteGui.TableOverview   5      W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )localclr-namespace:SQLiteGui     5
        � DataContext  5        SQLiteGui.TableDetails  5   
   .��5      �  � ColumnDefinitions 5   
   �� 5      $��5 �6   �� 5      $��* �6   �� 5      $��5 �6   �� 5      $��60 �6   �� 5      $��5 �6    � RowDefinitions 5   
   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5       $��25 �6   �� 5!      $��* �6   �� 5"      $��5 �6   .+�5$   
   &   System.Windows.Controls.DataGrid  -   5          Name$ Tables��5$         CanUserAddRows$ False��5&         
IsReadOnly$
 True��5'      $��1��5(      $��11��5)         Columns 5*      0  *System.Windows.Controls.DataGridTextColumn  5+      ,  &System.Windows.Controls.DataGridColumn   Header$ 
Table Name��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6X   1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 69   �� *
TableAlias+6X   
 � 	Resources
 5-         RowMenu �� RelativeSource��%�  &	        �� 5.        5/      �� *31PlacementTarget.(local:TableOverview.DataContext)+ �� *Self+.@�51      m� -   5        $��Truncate Table��51      67   m� 52      -   5        $��
Drop Table��52      63   m� 53      -   5        $��
Copy Table��53      63   m� 54      -   5        $��Rename Table��54      65   m� 55      -   5        $��	Add Table��55      62   56      57         RowStyle 58      �� 59       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�5:      ��  �� Property �@�ContextMenu �� Value# [  64   5;      5=      �� 5>   
   -   5        $��Truncate��5>      $��Stretch=�5?      $��Stretch=�5@      $��1��5A      $��3��5B      5C      �� 5D   
   -   5        $
��Drop��5D      $��Stretch=�5E      $��Stretch=�5F      $��5��5G      $��3��5H      5I      �� 5J   
   -	   5        $
��Copy��5J      $��Stretch=�5K      $��Stretch=�5L      $��3��5M      $��3��5N      5O      �� 5P   
   -
   5        $��Rename��5P      $��Stretch=�5Q      $��Stretch=�5R      $��7��5S      $��3��5T      5U      �� 5V   
   -   5        $��	Add Table��5V      $��Stretch=�5W      $��Stretch=�5X      $��7��5Y      $��3��5Z      5[      5\      5]      