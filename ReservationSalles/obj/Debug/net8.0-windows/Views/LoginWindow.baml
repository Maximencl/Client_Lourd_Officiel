   M S B A M L   `   `   `  ����L  HReservationSalles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:ReservationSalles.ViewModelsReservationSalles.ViewModels  5   	   K*clr-namespace:ReservationSalles.ConvertersReservationSalles.Converters  5   	   )    #ReservationSalles.Views.LoginWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   9vm*clr-namespace:ReservationSalles.ViewModels     5   	   ;conv*clr-namespace:ReservationSalles.Converters     5   	     � Title$  Login��5   	   $	��420��5   	   $	��300��6    � WindowStartupLocation$ CenterScreen=�5   	    � 	Resources 5      ?   9ReservationSalles.Converters.BooleanToVisibilityConverter   	BoolToVisF   @ReservationSalles.Converters.InverseBooleanToVisibilityConverter  InvBoolToVis��%:   &	        &	        5   
   6,     5   
   63   5       � DataContext 5      1   +ReservationSalles.ViewModels.LoginViewModel  5   
   .��5      � ���#F0F0F0$��10q�6     � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5#   
   �� $��Veuillez vous connecter��6   $��18(�5$      $
��Bold"�5%      $��Center=�5&      $��0,0,0,10q�5'      $��0��5(      �� 5+   
   
�������
�������$��1q�5-       �� CornerRadius$ 8��5.       �� Padding$ 20q�5/      $��1��50      ._�53      �  54      �� 55      $
��Auto �6$   �� 57      $
��Auto �6$   �� 59      $
��Auto �6$    � ColumnDefinitions 5<      �� 5=      $
��Auto �6'   �� 5?      $	��200 �6'   �� 5A      $
��Auto �6'   .+�5F      �� $��Identifiant :��6   $��Center=�5G      $��0,0,10,0q�5H      $��0��5I      $��0��6)   �� 5K      $��25��5L      $	��0,5q�5M      $��0��5N      $��1��6'   $��2��5O      ��5K      �� *
Username+5O      �� 5R      $��Mot de passe :��6   $��Center=�5S      $��	0,10,10,0q�5T      $��1��5U      $��0��6)   G� 5X      -   5         G�Name$ PwdBox��5X      $��25��5Y      $��0,10,0,0q�5Z      $��1��5]      $��1��6+   y�5\      �� *IsPasswordVisible+	 �� 	Converter#	 [ 5]   +   �� 5_      -   5        
 ��Name$
 TxtPwd��5_      $��25��5`      $��0,10,0,0q�5a      $��1��5d      $��1��6'   ��5b      �� *
Password+y�5c      �� *IsPasswordVisible+#	 [  5d   '   �� 5f      $��30��6   $��25��5g      $
��👁��5h      $��5,10,0,0q�5j      $��1��5k      $��2��6&   ��5i      �� *!TogglePasswordVisibilityCommand+5k   &   �� 5n      $��Se connecter��6   $	��120��5o      $��0,10,0,0q�5p      $��2��5r      $��1��5s      $
��Left=�5t      ��5q      �� *LoginCommand+5t      5u      5v      �� 5y   
   
���  ��$��Center=�5{      $��0,10,0,0q�5|      $��2��5}      ��5y      �� *ErrorMessage+5}      5~      5      