   M S B A M L   `   `   `  ����D  @SQLiteGui, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:SQLiteGui	SQLiteGui  5
   	   !   SQLiteGui.InputUpdateWindow   5   	   W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:SQLiteGui     5
   	     � Title$  Update Item��5   	   $	��300��5   	   $	��450��5	   	    � DataContext 5        SQLiteGui.UpdateItem  5   
   .��5      �  � ColumnDefinitions 5   
   �� 5      $��5 �6   �� 5      $��* �6   �� 5      $��5 �6   �� 5      $��60 �6   �� 5      $��5 �6    � RowDefinitions 5   
   �� 5      $��5 �6   �� 5      $��10 �6   �� 5      $��20 �6   �� 5      $��10 �6   �� 5      $��20 �6   �� 5      $��10 �6   �� 5      $��* �6   �� 5      $��5 �6   .+�5    
   &   System.Windows.Controls.DataGrid  -   5        $��DtGrdUpdate��5       $��1��5!      $��Stretch=�5"      $��1��5#      $��6��5$         AutoGenerateColumns$ False��5%         CanUserAddRows$ False��5&      $��Stretch=�5'         Columns 5(      0  *System.Windows.Controls.DataGridTextColumn  5)      ,  &System.Windows.Controls.DataGridColumn   Header$ Header Name��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6f   1  +System.Windows.Controls.DataGridBoundColumn	   Binding	 6:   �� *
HeaderName+
 �� Mode$
 OneWay=�6f     5*      $ 	Data Type��6%   $ * 6a   	 68   �� *
DataType+$
 OneWay=�6a     5+      $ Value��6%   $ * 5-   %   	 5,   %   �� *Value+$
 TwoWay=� �� NotifyOnSourceUpdated$
 True�� �� UpdateSourceTrigger$ PropertyChanged=�5-   %   5/      �� 50   
   -   5        $
��Okay��50      $��3��6    $��Stretch=�60   $��2��6N   $��Stretch=�6[   51      �� 52   
   -   5        $��Cancel��52      $��3��6"   $��Stretch=�62   $��4��6P   $��Stretch=�6]   53      54      55      