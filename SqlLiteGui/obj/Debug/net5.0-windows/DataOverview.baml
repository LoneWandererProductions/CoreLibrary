   M S B A M L   `   `   `  ����D  @SQLiteGui, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:SQLiteGui	SQLiteGui  5          SQLiteGui.DataOverview   5      W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -	sqLiteGuiclr-namespace:SQLiteGui     5        � DataContext  5      ��  �� RelativeSource ��  �� Mode$
 Self=�.��5      �  � RowDefinitions 5   
   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��5 �6   �� 5      $��25 �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��5 �6   �� 5      $��80 �6   �� 5      $��5 �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ 	TableData��5         CanUserAddRows$ False��5         AutoGenerateColumns$
 True��5      $��11��5         
IsReadOnly$
 True��5       	   ColumnWidth5  /System.Windows.Controls.DataGridLengthConverter$	 * 5!      $��50��5"      $��50��5#      $
��True��5$      $
��Auto=�5%      $
��Auto=�5&      
   CanUserResizeColumns$

 True��5'         CanUserResizeRows$
 True��5(       � 	Resources 5)         RowMenu��%2  &	        �� 5*        5+      �� *64PlacementTarget.(sqLiteGui:DataOverview.DataContext)+ �� *Self+.@�5-      m� -   5        $��Update Item��5-      64   m� 5.      -   5        $��Add Item��5.      61   m� 5/      -   5        $��Delete Item��5/      64   50      51         RowStyle 52      �� 53       �� 
TargetType)  #System.Windows.Controls.DataGridRow"  6   .�54      ��  �� Property �@�ContextMenu �� Value# [  64   55      57      �� 58   
   -   5        $��Update��58      $��Stretch=�6"   $��Stretch=�6@   $��1��6\   $��2��6i   59      �� 5:   
   -   5        $��Delete��5:      $��Stretch=�6"   $��Stretch=�6@   $��3��6\   $��2��6i   5;      �� 5<   
   -   5        $	��Add��5<      $��Stretch=�6   $��Stretch=�6=   $��5��6Y   $��2��6f   5=      5>      5?      