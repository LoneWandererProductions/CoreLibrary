﻿#pragma checksum "..\..\..\TableOverview.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "1909577E5CDF2A180C5EE2361F9E109F9DA9B80F"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using SQLiteGui;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SQLiteGui {
    
    
    /// <summary>
    /// TableOverview
    /// </summary>
    public partial class TableOverview : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 36 "..\..\..\TableOverview.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid Tables;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.17.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SQLiteGui;component/tableoverview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\TableOverview.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.17.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Tables = ((System.Windows.Controls.DataGrid)(target));
            
            #line 37 "..\..\..\TableOverview.xaml"
            this.Tables.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.DataGrd_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 49 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.TableTruncate_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 50 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.TableDrop_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 51 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.TableCopy_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 52 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.TableRename_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 53 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.AddTable_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 67 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.TableTruncate_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 73 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.TableDrop_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 79 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.TableCopy_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 85 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.TableRename_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 91 "..\..\..\TableOverview.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.AddTable_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
