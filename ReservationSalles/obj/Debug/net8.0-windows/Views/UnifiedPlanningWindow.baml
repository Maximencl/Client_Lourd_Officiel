   M S B A M L   `   `   `  ����  ReservationSalles3    -ReservationSalles.Views.UnifiedPlanningWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Planning Unifié��5   	   $	��700��5   	   $
��1100��5   	    � WindowStartupLocation$ CenterScreen=�5   	   .��5      �  � RowDefinitions 5	   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��Center=�5      $��10q�5      $��0��5      .+�5      �� $��Vue :��6   $��Center=�6%   $��0,0,5,0q�6@   �� 5      -   5         ��Name$ 	ModeCombo��5      $	��100��6*   $��0,0,20,0q�66   .@�5      �� $��Semaine��6   �� 5      $
��Mois��6   5      �� 5      -   5        $��Précédent��5      $��90��5      $��0,0,5,0q�5       5!      �� 5"      -   5         ��Name$ 
TitleLabel��5"      $
��Bold"�5#      $	��250��5$       �� TextAlignment$ Center=�5%      $��Center=�5&      $��0,0,5,0q�5'      �� 5(      -   5        $��Suivant��5(      $��90��5)      $��0,0,20,0q�5*      5+      �� 5.      $��	Filtrer :��6   $��Center=�6)   $��0,0,10,0q�6D   �� 5/      -   5        $ FilterCombo��5/      $	��120��6,   $��0,0,10,0q�68   .@�51      �� $
��Tous��6   �� 52      $��Utilisateur��6   �� 53      $��Salle��6   54      �� 55      -   5         ��Name$ 
FilterText��55      $	��120��6*   $��0,0,10,0q�66    � ToolTip$* $Tapez un nom/prénom ou une salle...��57      �� 5:      -   5        $��Exporter CSV��5:      $	��100��6,   $��0,0,5,0q�5;      5<      �� 5>      -   5        $��Imprimer��5>      $��80��6(   5?      5@      �� 5C   
   $��1��6   $
��Auto=�5D      $
��Auto=�5E      $��10q�5F      .��5H      � -	   5        	 �Name$	 RootGrid��5H      5I      5J      5K      