   M S B A M L   `   `   `  ����L  HReservationSalles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:ReservationSalles.ViewsReservationSalles.Views  5   	   K*clr-namespace:ReservationSalles.ViewModelsReservationSalles.ViewModels  5   	   K*clr-namespace:ReservationSalles.ConvertersReservationSalles.Converters  5   	   (    "ReservationSalles.Views.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   7local%clr-namespace:ReservationSalles.Views     5   	   9vm*clr-namespace:ReservationSalles.ViewModels     5   	   ;conv*clr-namespace:ReservationSalles.Converters     5   	     � Title$  Gestion des Réservations��5
   	   $	��600��5   	   $
��1000��6    � WindowStartupLocation$ CenterScreen=�6#    � 	Resources 5      ?   9ReservationSalles.Converters.BooleanToVisibilityConverter   	BoolToVis��%   &	          5   
   6,   5       � DataContext 5      0   *ReservationSalles.ViewModels.MainViewModel  5   
   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,0,0,10q�6W   .+�5      �� -   5        $��Administration��5      $	��120��6.   $��0,0,10,0q�6:   y�5      �� *IsCurrentUserAdmin+ �� 	Converter# [  �� 5      -   5        $��Planning Schématique��5      $	��150��64   $��0,0,10,0q�6@   6R   �� 5       -   5        $��Déconnexion��5       $	��120��6+   67   5!      � 5#   
   $��1��6    � ColumnDefinitions 5$      �� 5%      $��2* �6#   �� 5&      $��3* �6#   .+�5)      �� $��0��6   .+�5*      �� $��Salles disponibles :��6   $
��Bold"�68   $��0,0,0,5q�6J   �� 5+       �� DisplayMemberPath$
 Name��6^   $	��200��6w   ��6   �� *Rooms+	 �� SelectedItem	 68   �� *SelectedRoom+6w   �� 5,      $��0,5,0,0q�6   ��5-      �� 5.      
 �� 
TargetType"
 ��6    .�5/      ��  �� Property � �� �� Value 65   �� *SelectedRoom.Capacity+ �� StringFormat$ Capacité : {0} personnes�� �� TargetNullValue$  ���� 50       � ��$ Normal$�6:    �� Triggers 51      �� 52   "    �� Binding 6.   �� *SelectedRoom+ �� Value 6O   Z� .a�53   &   ��  � ��$5 /Sélectionnez une salle pour voir la capacité.��6=   �� 54   &    � ��$ Italic$�6B   55   #   57      59      5:      �� 5<      $��1��6   $
��Auto=�6+   $��20,0,0,0q�6N   .��5=      �� .+�5>      �� $��Nouvelle Réservation��6    $
��Bold"�6<   $��14(�6N   $��0,0,0,10q�6\   �� 5?      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5@      �� $��Jour :��6$   $	��100��62   $��Center=�6>   (  "System.Windows.Controls.DatePicker  5A      $��Center=�6N      SelectedDate 6%   �� *ReservationDate+6N   5B      �� 5C      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5D      �� $��Heure début :��6$   $	��100��69   $��Center=�6E   �� 5E      $	��100��6#   $��Center=�6�   ��6/   �� *StartTimeOptions+	 6X   �� *SelectedStartTime+6�   5F      �� 5G      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5H      �� $��Heure fin :��6$   $	��100��67   $��Center=�6C   �� 5I      $	��100��6#   $��Center=�6   ��6/   �� *EndTimeOptions+	 6V   �� *SelectedEndTime+6   5J      �� 5K      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5L      �� $��Nom :��6$   $	��100��61   $��Center=�6=   �� 5M      $	��150��6"   $��Center=�6u   ��6.   �� *AttendeeLastName+ �� UpdateSourceTrigger$ PropertyChanged=�6u   5N      �� 5O      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5P      �� $��	Prénom :��6$   $	��100��64   $��Center=�6@   �� 5Q      $	��150��6"   $��Center=�6v   ��6.   �� *AttendeeFirstName+$ PropertyChanged=�6v   5R      �� 5S      $ 
Horizontal=�6!   $	��0,5q�6:   .+�5T      �� $��Objet :��6$   $	��100��63   $��Center=�6?   �� 5U      $	��150��6"   $��Center=�6s   ��6.   �� *MeetingSubject+$ PropertyChanged=�6s   5V      �� 5W      $��Ajouter Réservation��6   $	��150��6;   $��
100,10,0,0q�6G   $
��Left=�6�   ��6[   �� *AddReservationCommand+6�   �� 5X      
��� � �$��
100,10,0,0q�6L   $
��Wrap=�6`   ��6    �� *	Message+6`   �� 5Z      $��	0,20,0,10q�6    �� 5[      $��Liste des Réservations :��6    $
��Bold"�6@   $��0,0,0,5q�6R   &   System.Windows.Controls.DataGrid  5\      -   5          Name$ ReservationsDataGrid��5\         AutoGenerateColumns$ False��5^      $	��150��5`      $	��300��6/      
IsReadOnly$
 True��5a      "   VerticalScrollBarVisibility$
 Auto=�5b      ��5]      �� *Reservations+	 5_      �� *SelectedReservation+   Columns 5d      0  *System.Windows.Controls.DataGridTextColumn  5e      ,  &System.Windows.Controls.DataGridColumn   Header$ Salle��61      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6^   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6@   �� *	Room.Name+6^     5f      $ Début��61   $
 1.5* 6�    6@   �� *	StartTime+$ {0:dd/MM/yyyy HH:mm}��6�     5g      $	 Fin��61   $
 1.5* 6    6>   �� *	EndTime+$ {0:dd/MM/yyyy HH:mm}��6     5h      $ Gérant��61   $ * 6A      ElementStyle 5i   "   �� 5j   &   "
 ��6,   .�5k   *   ��  � �� 5l   .   `� 5m   2   $ {0} {1}��6?   .1�5n   6   ��  �� Path$ AttendeeLastName�6>   �� 5o   6   $ AttendeeFirstName�6>   5p   3   5r   +   5s   '   5u        5v      $ Objet��61   $
 1.5* 6c    6@   �� *MeetingSubject+6c   5x      5y      5z      5{      �� 5}   
   $��2��6   $ 
Horizontal=�6"   $
��Left=�6;   $��0,10,0,0q�6V   .+�5~      �� $��Supprimer la réservation��6   $	��180��68   ��6D   �� *RemoveReservationCommand+�� 5      $��Rafraîchir��6   $	��100��6*   $��10,0,0,0q�66   ��6H   �� *RefreshCommand+5�      5�      5�      