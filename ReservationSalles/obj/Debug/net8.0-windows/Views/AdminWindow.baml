   M S B A M L   `   `   `  ����L  HReservationSalles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:ReservationSalles.ViewModelsReservationSalles.ViewModels  5   	   )    #ReservationSalles.Views.AdminWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   9vm*clr-namespace:ReservationSalles.ViewModels     5   	     � Title$  Administration��5   	   $	��600��5   	   $	��800��6    � WindowStartupLocation$ CenterScreen=�6#    � DataContext 5      1   +ReservationSalles.ViewModels.AdminViewModel  5	   
   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   .@�5      �� $��Utilisateurs��6   .��5      � $��5q�6    5      �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $��* �6(   �� 5      $
��Auto �6(   .+�5      �� $��0��6   
�������$��1q�6=    �� Padding$ 10q�6Q   $��0,0,0,5q�6^   ._�5      ��  �� Orientation$ Vertical=�6%   .+�5      �� $%��Ajouter un nouvel utilisateur :��6(   $
��Bold"�6O   $��0,0,0,5q�6a   � 5       � ColumnDefinitions 5    "   �� 5!   &   $
��Auto �67   �� 5"   &   $��* �67   �� 5#   &   $
��Auto �67   �� 5$   &   $��* �67    5&   "   �� 5'   &   $
��Auto �64   �� 5(   &   $
��Auto �64   �� 5)   &   $
��Auto �64   .+�5,   "   �� $��0��6,   $��0��69   $��	Username:��6I   $��Center=�6Z   $��0,0,5,0q�6u   �� 5-   "   $��0��6*   $��1��67   $	��150��6G   $	��0,2q�6S   ��5.   *   �� *NewUsername+ �� UpdateSourceTrigger$ PropertyChanged=��� 50   "   $��1��6,   $��0��69   $��	Password:��6I   $��Center=�6Z   $��0,0,5,0q�6u   �� 51   "   $��1��6*   $��1��67   $	��150��6G   $	��0,2q�6S   ��52   *   �� *NewPassword+$ PropertyChanged=��� 54   "   $��0��6,   $��2��69   $��Rôle:��6I   $��Center=�6V   $��10,0,5,0q�6q   �� 55   "   $��0��6+   $��3��68   $	��100��6H   $	��0,2q�6T    �� SelectedValuePath$ Content��57   +   	 �� SelectedValue	 56   +   �� *	NewRole+$ PropertyChanged=�.@�58   &   �� $��Admin��63   �� 59   &   $
��User��63   5:   #   �� 5<   "   $��2��6)   $��3��66   $��Ajouter Utilisateur��6F   $��Right=�6d   $��0,10,0,0q�6�   $	��130��5>   )   ��5=   )   �� *AddUserCommand+5>   )   5?      5@      5A      �� 5C      $��1��6    
��� � �$��0,5,0,5q�6g   ��6-   �� *UserManagementMessage+6g   &   System.Windows.Controls.DataGrid  5E      $��2��6   $��0,5,0,0q�6,   
   AutoGenerateColumns$
 False��5G         
IsReadOnly$
 True��5I      ��5F      �� *Users+ �� SelectedItem 5H      �� *SelectedUser+   Columns 5J      0  *System.Windows.Controls.DataGridTextColumn  5K      ,  &System.Windows.Controls.DataGridColumn   Header$ Username��61      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6`   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6C   �� *
Username+6`     5L      $ Rôle��61   $ * 6X    6?   �� *Role+6X   5N      �� 5P      $��3��6   $+��%Supprimer l'utilisateur sélectionné��5Q      $��Right=�5S      $��0,10,0,0q�5T      ��5R      �� *RemoveUserCommand+5T      5U      5V      �� 5X      $��Salles��6   .��5Y      � $��5q�6    5Z      �� 5[      $
��Auto �6(   �� 5\      $
��Auto �6(   �� 5]      $��* �6(   �� 5^      $
��Auto �6(   .+�5a      �� $��0��6   
�������$��1q�6=   $ 10q�6Q   $��0,0,0,5q�6^   ._�5b      �� $ Vertical=�6%   .+�5c      �� $"��Ajouter une nouvelle salle :��6(   $
��Bold"�6L   $��0,0,0,5q�6^   � 5d       5e   "   �� 5f   &   $
��Auto �67   �� 5g   &   $��* �67   �� 5h   &   $
��Auto �67   �� 5i   &   $��* �67   �� 5j   &   $
��Auto �67    5l   "   �� 5m   &   $
��Auto �64   �� 5n   &   $
��Auto �64   .+�5q   "   �� $��0��6,   $��0��69   $
��Nom:��6I   $��Center=�6U   $��0,0,5,0q�6p   �� 5r   "   $��0��6*   $��1��67   $	��200��6G   $	��0,2q�6S   ��5s   *   �� *NewRoomName+$ PropertyChanged=��� 5u   "   $��0��6,   $��2��69   $��
Capacité:��6I   $��Center=�6Z   $��10,0,5,0q�6u   �� 5v   "   $��0��6*   $��3��67   $��80��6G   $	��0,2q�6R   ��5w   *   �� *NewRoomCapacityString+$ PropertyChanged=��� 5y   "   $��1��6)   $��4��66   $��Ajouter Salle��6F   $��Right=�6^   $��	10,10,0,0q�6z   $	��130��5{   )   ��5z   )   �� *AddRoomCommand+5{   )   5|      5}      5~      �� 5�      $��1��6    
��� � �$��0,5,0,5q�6g   ��6-   �� *RoomManagementMessage+6g     5�      $��2��6   $��0,5,0,0q�6,   $
 False��5�      $
 True��5�      ��5�      �� *Rooms+ 5�      �� *SelectedRoom+ 5�        5�      $	 Nom��61   $ * 6W    6>   �� *Name+6W     5�      $ 	Capacité��61   $	 100 6`    6C   �� *
Capacity+6`   5�      �� 5�      $��3��6   $'��!Supprimer la salle sélectionnée��5�      $��Right=�5�      $��0,10,0,0q�5�      ��5�      �� *RemoveRoomCommand+5�      5�      5�      �� 5�      $��Réservations��6   .��5�      � $��5q�6    5�      �� 5�      $
��Auto �6(   �� 5�      $��* �6(   �� 5�      $
��Auto �6(   .+�5�      �� $��0��6    
��� � �$��0,5,0,5q�6n   ��6-   �� *ReservationManagementMessage+6n     5�      $��1��6   $��0,5,0,0q�6,   $
 False��5�      $
 True��5�      ��5�      �� *Reservations+ 5�      �� *SelectedReservation+ 5�        5�      $ Salle��61   $
 1.5* 6^    6@   �� *	Room.Name+6^     5�      $ Début��61   $
 1.5* 6�    6@   �� *	StartTime+ �� StringFormat$ {0:dd/MM/yyyy HH:mm}��6�     5�      $	 Fin��61   $
 1.5* 6    6>   �� *	EndTime+$ {0:dd/MM/yyyy HH:mm}��6     5�      $ Gérant��61   $ * 6A      ElementStyle 5�   "   �� 5�   &    �� 
TargetType" ��6,   .�5�   *   ��  �� Property � �� �� Value 5�   .   `� 5�   2   $ {0} {1}��6?   .1�5�   6   ��  �� Path$ AttendeeLastName�6>   �� 5�   6   $ AttendeeFirstName�6>   5�   3   5�   +   5�   '   5�        5�      $ Objet��61   $ 2* 6c    6@   �� *MeetingSubject+6c   5�      �� 5�      $��2��6!   $ 
Horizontal=�6.   $��Right=�6G   $��0,10,0,0q�6c   .+�5�      �� $��Modifier Réservation��6!   $��0,0,10,0q�6m   ��6@   �� *ModifyReservationCommand+6m   �� 5�      $��Supprimer Réservation��6!   ��6A   �� *RemoveReservationCommandAdmin+5�      5�      5�      5�      �� 5�   
   $��1��6   $%��Rafraîchir Toutes les Données��6   $
��Left=�6l   $��	10,10,0,0q�6�   ��6F   �� *RefreshAllCommand+6�   5�      5�      