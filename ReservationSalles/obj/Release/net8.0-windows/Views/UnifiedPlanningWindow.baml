   M S B A M L   `   `   `  ����   ReservationSalles3    -ReservationSalles.Views.UnifiedPlanningWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Planning Unifié��$	��700��$
��1100�� � WindowStartupLocation$ CenterScreen=�.���  � RowDefinitions �� $
��Auto ��� $��* �.+���  �� Orientation$ 
Horizontal=�$��Center=�$��10q�$��0��.+��� $��Vue :��$��Center=�$��0,0,5,0q��� -    ��Name$ 	ModeCombo��$	��100��$��0,0,20,0q�.@��� $��Semaine���� $
��Mois���� -   $��Précédent��$��90��$��0,0,5,0q��� -    ��Name$ 
TitleLabel��$
��Bold"�$	��250�� �� TextAlignment$ Center=�$��Center=�$��0,0,5,0q��� -   $��Suivant��$��90��$��0,0,20,0q��� $��	Filtrer :��$��Center=�$��0,0,10,0q��� -   $ FilterCombo��$	��120��$��0,0,10,0q�.@��� $
��Tous���� $��Utilisateur���� $��Salle���� -    ��Name$ 
FilterText��$	��120��$��0,0,10,0q� � ToolTip$* $Tapez un nom/prénom ou une salle...���� -   $��Exporter CSV��$	��100��$��0,0,5,0q��� -   $��Imprimer��$��80���� $��1��$
��Auto=�$
��Auto=�$��10q�.��� -	   	 �Name$	 RootGrid��