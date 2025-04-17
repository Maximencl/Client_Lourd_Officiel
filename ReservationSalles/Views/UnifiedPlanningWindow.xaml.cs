using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input; // Assurez-vous que ceci est présent pour MouseButtonEventArgs
using Microsoft.Win32;
using ReservationSalles.Models;
using ReservationSalles.Services; // <-- Important pour que DataService soit reconnu
using System.Globalization; // Ajout pour CultureInfo et DateTimeFormatInfo si besoin
using System.Windows.Shapes; // Ajout pour Rectangle si besoin

namespace ReservationSalles.Views
{
    public partial class UnifiedPlanningWindow : Window
    {
        // Pour la vue semaine : 7 jours (lundi..dimanche) - adapté suite à l'analyse du code
        private const int NB_DAYS_IN_WEEK = 7; // Explicitement 7 jours
        private const int lowestHour = 8;   // début de journée = 8h
        private const int highestHour = 19; // fin de journée = 19h
        private const int halfHourCount = (highestHour - lowestHour) * 2; // (19-8)*2 = 22 créneaux par jour

        private string _currentMode = "Semaine"; // "Semaine" ou "Mois"
        private DateTime _currentDate;
        // Initialiser les listes pour éviter les null potentiels avant LoadAllData
        private List<Reservation> _allReservations = new List<Reservation>();
        private List<Reservation> _displayedReservations = new List<Reservation>();

        public UnifiedPlanningWindow()
        {
            InitializeComponent();
            LoadAllData(); // Charger les données


            // S'assurer que _displayedReservations est initialisé même si _allReservations est vide
            _displayedReservations = new List<Reservation>(_allReservations ?? Enumerable.Empty<Reservation>());

            ModeCombo.SelectedIndex = 0; // Mode "Semaine" par défaut
            _currentDate = GetMonday(DateTime.Now); // Commence à la semaine actuelle
            FilterCombo.SelectedIndex = 0; // Filtre "Tous" par défaut
            RefreshPlanning(); // Affichage initial
        }

        // Charge les données initiales (toutes les réservations)
        private void LoadAllData()
        {
            try
            {
                _allReservations = DataService.GetAllReservations() ?? new List<Reservation>(); // Assurer non null via ??

                // --- LOG AJOUTÉ 1 (Utile pour débogage) ---
                Logger.Log($"LoadAllData in UnifiedPlanningWindow loaded {_allReservations.Count} reservations. Dates:");
                foreach (var res in _allReservations.OrderBy(r => r?.StartTime ?? DateTime.MaxValue)) // Trier pour lisibilité + null check
                {
                    Logger.Log($"- ID: {res?.Id}, Start: {res?.StartTime:yyyy-MM-dd HH:mm}, End: {res?.EndTime:HH:mm}, Room: {res?.Room?.Name}"); // Ajout null checks
                }
                // --- FIN LOG AJOUTÉ 1 ---

                Logger.Log($"Data loaded successfully for UnifiedPlanningWindow ({_allReservations.Count} reservations).");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur chargement données planning: {ex.Message}", "Erreur");
                Logger.Log($"Error loading planning data: {ex.Message}");
                _allReservations = new List<Reservation>(); // Initialiser à vide en cas d'erreur grave
            }
        }

        // Quand on change "Semaine" <-> "Mois"
        private void ModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeCombo.SelectedItem is ComboBoxItem item)
            {
                string newMode = item.Content?.ToString() ?? "Semaine";

                if (newMode != _currentMode) // Seulement si le mode change
                {
                    _currentMode = newMode;
                    // Ajuster la date de référence si nécessaire
                    if (_currentMode == "Semaine")
                    {
                        _currentDate = GetMonday(_currentDate); // Se caler sur le lundi de la date actuelle
                    }
                    else // "Mois"
                    {
                        _currentDate = new DateTime(_currentDate.Year, _currentDate.Month, 1); // Se caler sur le 1er du mois
                    }
                    ApplyFilter(); // Appliquer le filtre mettra à jour la période ET rafraîchira
                }
            }
        }

        // *** CORRECTION APPLIQUÉE ICI ***
        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == "Semaine")
                _currentDate = _currentDate.AddDays(-NB_DAYS_IN_WEEK); // Utilise la constante 7 jours
            else
                _currentDate = _currentDate.AddMonths(-1);

            ApplyFilter(); // Appelle ApplyFilter pour re-filtrer et redessiner
        }

        // *** CORRECTION APPLIQUÉE ICI ***
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMode == "Semaine")
                _currentDate = _currentDate.AddDays(NB_DAYS_IN_WEEK); // Utilise la constante 7 jours
            else
                _currentDate = _currentDate.AddMonths(1);

            ApplyFilter(); // Appelle ApplyFilter pour re-filtrer et redessiner
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter(); // Réappliquer le filtre quand la sélection change
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(); // Réappliquer le filtre quand le texte change
        }

        // Applique les filtres (période + texte) et met à jour _displayedReservations
        private void ApplyFilter()
        {
            DateTime startPeriod, endPeriod;
            string filterType = (FilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tous";
            string filterText = FilterText.Text?.Trim().ToLowerInvariant() ?? "";

            // S'assurer que _allReservations n'est pas null
            IEnumerable<Reservation> filtered = _allReservations ?? Enumerable.Empty<Reservation>();

            // 1. Filtrer par période (Semaine ou Mois)
            if (_currentMode == "Semaine")
            {
                startPeriod = GetMonday(_currentDate);
                endPeriod = startPeriod.AddDays(NB_DAYS_IN_WEEK); // Utilise la constante 7 jours
            }
            else // Mois
            {
                startPeriod = new DateTime(_currentDate.Year, _currentDate.Month, 1);
                endPeriod = startPeriod.AddMonths(1);
            }
            // Filtrer les réservations qui intersectent la période visible
            // Ajout de null checks pour la robustesse
            filtered = filtered.Where(r => r != null && r.EndTime > startPeriod && r.StartTime < endPeriod);

            // 2. Appliquer le filtre texte si nécessaire
            if (filterType != "Tous" && !string.IsNullOrEmpty(filterText))
            {
                if (filterType == "Utilisateur")
                {
                    filtered = filtered.Where(r => r != null &&
                           ((r.AttendeeLastName?.ToLowerInvariant().Contains(filterText) ?? false) ||
                           (r.AttendeeFirstName?.ToLowerInvariant().Contains(filterText) ?? false))
                       );
                }
                else if (filterType == "Salle")
                {
                    filtered = filtered.Where(r => r != null && r.Room != null &&
                                                    (r.Room.Name?.ToLowerInvariant().Contains(filterText) ?? false));
                }
                else if (filterType == "Objet") // Ajout filtre Objet si besoin
                {
                    filtered = filtered.Where(r => r != null &&
                                                    (r.MeetingSubject?.ToLowerInvariant().Contains(filterText) ?? false));
                }
            }

            // Mettre à jour la liste qui sera utilisée pour l'affichage
            _displayedReservations = filtered.ToList();

            // --- LOG AJOUTÉ 2 (Utile pour débogage) ---
            Logger.Log($"ApplyFilter completed. Mode: {_currentMode}. Period: {startPeriod:yyyy-MM-dd} to {endPeriod:yyyy-MM-dd}. Filter Type: {filterType}, Text: '{filterText}'. Displaying {_displayedReservations.Count} reservations:");
            foreach (var res in _displayedReservations.OrderBy(r => r?.StartTime ?? DateTime.MaxValue))
            {
                Logger.Log($"-- ID: {res?.Id}, Start: {res?.StartTime:yyyy-MM-dd HH:mm}, Room: {res?.Room?.Name}");
            }
            // --- FIN LOG AJOUTÉ 2 ---

            // Rafraîchir l'affichage graphique
            RefreshPlanning();
        }

        // Efface et reconstruit la grille visuelle
        private void RefreshPlanning()
        {
            RootGrid.Children.Clear();
            RootGrid.RowDefinitions.Clear();
            RootGrid.ColumnDefinitions.Clear();

            if (_currentMode == "Semaine")
            {
                BuildWeeklyGrid();
            }
            else // "Mois"
            {
                BuildMonthlyGrid();
            }
        }

        #region Weekly View
        private void BuildWeeklyGrid()
        {
            DateTime monday = GetMonday(_currentDate);
            DateTime sundayEnd = monday.AddDays(NB_DAYS_IN_WEEK);

            TitleLabel.Text = $"Semaine du {monday:dd/MM/yyyy}";

            // Structure de la Grille
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) }); // Heures
            for (int d = 0; d < NB_DAYS_IN_WEEK; d++) RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Jours

            RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) }); // Entete Jours
            for (int i = 0; i < halfHourCount; i++) RootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) }); // Créneaux

            // Remplissage Entetes (Jours et Heures)
            for (int d = 0; d < NB_DAYS_IN_WEEK; d++)
            {
                DateTime day = monday.AddDays(d);
                TextBlock dayLabel = new TextBlock { Text = day.ToString("ddd dd/MM", CultureInfo.CurrentCulture), FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                if (day.Date == DateTime.Today) dayLabel.Foreground = Brushes.Red;
                Grid.SetColumn(dayLabel, d + 1); Grid.SetRow(dayLabel, 0); RootGrid.Children.Add(dayLabel);
            }
            TimeSpan currentTime = TimeSpan.FromHours(lowestHour);
            for (int i = 0; i < halfHourCount; i++)
            {
                TextBlock timeLabel = new TextBlock { Text = currentTime.ToString(@"hh\:mm"), HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
                Grid.SetColumn(timeLabel, 0); Grid.SetRow(timeLabel, i + 1); RootGrid.Children.Add(timeLabel);
                currentTime = currentTime.Add(TimeSpan.FromMinutes(30));
            }

            // Quadrillage (visuel)
            for (int r = 0; r <= halfHourCount; r++) for (int c = 0; c <= NB_DAYS_IN_WEEK; c++)
                {
                    Border cellBorder = new Border { BorderBrush = Brushes.LightGray, BorderThickness = (c == 0 || r == 0) ? new Thickness(0) : new Thickness(0, 0, 1, 1) };
                    Grid.SetRow(cellBorder, r); Grid.SetColumn(cellBorder, c); RootGrid.Children.Add(cellBorder);
                    Panel.SetZIndex(cellBorder, -1);
                }

            // Placer les réservations filtrées
            PlaceWeeklyReservations(monday, sundayEnd);
        }

        private void PlaceWeeklyReservations(DateTime weekStart, DateTime weekEnd)
        {
            var colorMap = new Dictionary<string, Brush>();

            // S'assurer que la liste n'est pas null
            if (_displayedReservations == null)
            {
                Logger.Log("PlaceWeeklyReservations: _displayedReservations is null. Skipping.");
                return;
            }

            foreach (var res in _displayedReservations.Where(r => r != null)) // Itérer sur la liste déjà filtrée pour la période + null check
            {
                // --- LOG AJOUTÉ 3 (Utile pour débogage) ---
                Logger.Log($"PlaceWeekly: Processing Res ID {res.Id}, Start: {res.StartTime:yyyy-MM-dd HH:mm}, Room: {res.Room?.Name}");
                // --- FIN LOG AJOUTÉ 3 ---

                DateTime start = res.StartTime; // Pas besoin de clamper, car déjà filtré par ApplyFilter
                DateTime end = res.EndTime;

                // Vérifier si la réservation est dans les heures affichables (8h-19h)
                if (start.TimeOfDay >= TimeSpan.FromHours(highestHour) || end.TimeOfDay <= TimeSpan.FromHours(lowestHour)) continue;

                // Calcul colonne (0 = Lun ... 6 = Dim) -> +1 pour grille
                int dayIndex = ((int)start.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                int gridCol = dayIndex + 1;

                // Calcul ligne début et durée
                TimeSpan timeFromLowest = start.TimeOfDay.Subtract(TimeSpan.FromHours(lowestHour));
                // Gérer les cas où la réservation commence avant 8h
                int startSlotIndex = timeFromLowest.TotalMinutes < 0 ? 0 : (int)(timeFromLowest.TotalMinutes / 30);

                TimeSpan duration = end.TimeOfDay.Subtract(start.TimeOfDay);
                // Gérer les cas où la réservation finit après 19h ou commence avant 8h
                if (start.TimeOfDay < TimeSpan.FromHours(lowestHour))
                {
                    duration = end.TimeOfDay.Subtract(TimeSpan.FromHours(lowestHour));
                }
                if (end.TimeOfDay > TimeSpan.FromHours(highestHour))
                {
                    duration = TimeSpan.FromHours(highestHour).Subtract(start.TimeOfDay < TimeSpan.FromHours(lowestHour) ? TimeSpan.FromHours(lowestHour) : start.TimeOfDay);
                }

                int slotsToSpan = (int)Math.Ceiling(duration.TotalMinutes / 30);
                if (slotsToSpan <= 0) continue; // Si la durée visible est nulle ou négative

                int gridRow = startSlotIndex + 1; // +1 pour l'entête

                // Ajuster le span si ça dépasse la grille (1+22 lignes)
                if (gridRow + slotsToSpan > halfHourCount + 1)
                {
                    slotsToSpan = (halfHourCount + 1) - gridRow;
                }
                if (slotsToSpan <= 0) continue;

                // Création du visuel de la réservation
                string colorKey = res.Room?.Name?.ToLowerInvariant() ?? "default";
                Border reservationBorder = new Border
                {
                    BorderBrush = Brushes.DarkGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Background = GetColorForKey(colorKey, colorMap),
                    Margin = new Thickness(1),
                    Padding = new Thickness(2),
                    DataContext = res // Important pour le double clic
                };
                TextBlock info = new TextBlock
                {
                    Text = $"{res.Room?.Name}\n{res.StartTime:HH:mm}-{res.EndTime:HH:mm}\n{res.MeetingSubject}",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 9,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                reservationBorder.Child = info;
                reservationBorder.ToolTip = $"Salle: {res.Room?.Name}\nDe: {res.StartTime:dd/MM HH:mm} à {res.EndTime:dd/MM HH:mm}\nPar: {res.AttendeeLastName} {res.AttendeeFirstName}\nObjet: {res.MeetingSubject}";
                reservationBorder.MouseLeftButtonDown += Reservation_MouseDoubleClick;

                // Positionnement
                Grid.SetColumn(reservationBorder, gridCol);
                Grid.SetRow(reservationBorder, gridRow);
                Grid.SetRowSpan(reservationBorder, slotsToSpan);
                Panel.SetZIndex(reservationBorder, 10); // Au-dessus du quadrillage

                RootGrid.Children.Add(reservationBorder);
            }
        }
        #endregion

        #region Monthly View
        private void BuildMonthlyGrid()
        {
            DateTime firstOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            TitleLabel.Text = firstOfMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture); // Ajout année

            // Structure Grille
            for (int c = 0; c < 7; c++) RootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r < 7; r++) RootGrid.RowDefinitions.Add(new RowDefinition { Height = (r == 0) ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });

            // Entete Jours
            string[] daysHeader = { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
            for (int c = 0; c < 7; c++)
            {
                TextBlock header = new TextBlock { Text = daysHeader[c], FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 5, 0, 5) };
                Grid.SetRow(header, 0); Grid.SetColumn(header, c); RootGrid.Children.Add(header);
            }

            // Cellules Jours
            int startColumnOffset = ((int)firstOfMonth.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            int currentDay = 1;
            var colorMap = new Dictionary<string, Brush>(); // Une map pour tout le mois

            // S'assurer que _displayedReservations n'est pas null
            _displayedReservations ??= new List<Reservation>();

            for (int r = 1; r < 7; r++) // Lignes semaines
            {
                for (int c = 0; c < 7; c++) // Colonnes jours
                {
                    Border cellBorder = new Border { BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 1, 1) };
                    Grid.SetRow(cellBorder, r); Grid.SetColumn(cellBorder, c); RootGrid.Children.Add(cellBorder);

                    if ((r == 1 && c < startColumnOffset) || currentDay > daysInMonth)
                    {
                        cellBorder.Background = Brushes.WhiteSmoke; // Cellule vide
                    }
                    else
                    {
                        DateTime cellDate = new DateTime(_currentDate.Year, _currentDate.Month, currentDay);
                        StackPanel panel = new StackPanel { Margin = new Thickness(2) };
                        cellBorder.Child = panel;

                        TextBlock dayLabel = new TextBlock { Text = currentDay.ToString(), HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 3, 1) };
                        if (cellDate.Date == DateTime.Today) dayLabel.Foreground = Brushes.Red;
                        panel.Children.Add(dayLabel);

                        // Récupérer les réservations pour CE jour depuis la liste DÉJÀ FILTRÉE pour le mois
                        var dayReservations = _displayedReservations
                            .Where(res => res != null && res.StartTime.Date == cellDate.Date)
                            .OrderBy(res => res.StartTime);

                        // --- LOG AJOUTÉ 4 (Optionnel - peut être verbeux) ---
                        // Logger.Log($"BuildMonthlyGrid: Day {cellDate:yyyy-MM-dd}. Found {dayReservations.Count()} reservations in _displayedReservations.");
                        // --- FIN LOG AJOUTÉ 4 ---

                        foreach (var res in dayReservations)
                        {
                            string colorKey = res.Room?.Name?.ToLowerInvariant() ?? "default";
                            Border resBorder = new Border { Background = GetColorForKey(colorKey, colorMap), CornerRadius = new CornerRadius(2), Padding = new Thickness(2), Margin = new Thickness(0, 1, 0, 1), DataContext = res };
                            TextBlock resText = new TextBlock { Text = $"{res.StartTime:HH:mm} {res.Room?.Name}", FontSize = 9, TextTrimming = TextTrimming.CharacterEllipsis };
                            resBorder.Child = resText;
                            resBorder.ToolTip = $"({res.StartTime:HH:mm}-{res.EndTime:HH:mm}) {res.Room?.Name}: {res.MeetingSubject}";
                            resBorder.MouseLeftButtonDown += Reservation_MouseDoubleClick;
                            panel.Children.Add(resBorder);
                        }
                        currentDay++;
                    }
                }
            }
        }
        #endregion

        #region Actions (Export, Print, Edit)

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            // S'assurer que _displayedReservations n'est pas null avant de vérifier .Any()
            if (_displayedReservations == null || !_displayedReservations.Any())
            {
                MessageBox.Show("Aucune donnée à exporter.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = $"planning_{_currentDate:yyyyMM}.csv", // Nom de fichier basé sur mois/année courante
                Filter = "Fichier CSV|*.csv",
                Title = "Exporter le planning visible"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("Salle;Date Debut;Heure Debut;Date Fin;Heure Fin;Nom;Prenom;Objet"); // Entete

                    // Exporter les réservations triées depuis la liste affichée
                    foreach (var res in _displayedReservations.OrderBy(r => r?.StartTime ?? DateTime.MaxValue)) // Tri + null check
                    {
                        if (res == null) continue; // Sécurité supplémentaire
                        // Formatage sécurisé avec remplacement des guillemets et gestion des nulls
                        csv.AppendFormat("\"{0}\";{1:dd/MM/yyyy};{1:HH:mm};{2:dd/MM/yyyy};{2:HH:mm};\"{3}\";\"{4}\";\"{5}\"\n",
                                         res.Room?.Name?.Replace("\"", "\"\"") ?? "",
                                         res.StartTime,
                                         res.EndTime,
                                         res.AttendeeLastName?.Replace("\"", "\"\"") ?? "",
                                         res.AttendeeFirstName?.Replace("\"", "\"\"") ?? "",
                                         res.MeetingSubject?.Replace("\"", "\"\"") ?? "");
                    }
                    File.WriteAllText(sfd.FileName, csv.ToString(), Encoding.UTF8); // Utiliser UTF8 pour compatibilité
                    MessageBox.Show($"Export vers '{sfd.FileName}' terminé.", "Export CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Log($"Planning exported to {sfd.FileName}");
                }
                catch (Exception ex) { MessageBox.Show($"Erreur export CSV: {ex.Message}", "Erreur"); Logger.Log($"Error exporting CSV: {ex.Message}"); }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                try
                {
                    pd.PrintVisual(RootGrid, $"Planning - {TitleLabel.Text}");
                    Logger.Log("Planning printed.");
                }
                catch (Exception ex) { MessageBox.Show($"Erreur impression: {ex.Message}", "Erreur"); Logger.Log($"Error printing: {ex.Message}"); }
            }
        }

        // Gestion Double Clic pour édition
        private void Reservation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement element && element.DataContext is Reservation res)
            {
                Logger.Log($"Double click detected on reservation ID {res.Id}");
                // Créer une copie pour l'édition afin de ne pas modifier l'original si on annule
                var resCopy = new Reservation
                {
                    Id = res.Id,
                    Room = res.Room, // Peut être null si la salle a été supprimée entre temps
                    StartTime = res.StartTime,
                    EndTime = res.EndTime,
                    AttendeeFirstName = res.AttendeeFirstName ?? "",
                    AttendeeLastName = res.AttendeeLastName ?? "",
                    MeetingSubject = res.MeetingSubject ?? ""
                };

                // Vérifier si la salle existe toujours (important si des opérations concurrentes sont possibles)
                if (resCopy.Room == null)
                {
                    MessageBox.Show("La salle associée à cette réservation n'existe plus.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Logger.Log($"Edit attempt failed for reservation {res.Id}: Associated room is null.");
                    return; // Ne pas ouvrir la fenêtre d'édition
                }

                EditReservationWindow editWin = new EditReservationWindow(resCopy);
                if (editWin.ShowDialog() == true) // L'utilisateur a cliqué "Sauvegarder" dans la fenêtre d'édition
                {
                    try
                    {
                        // Tenter la mise à jour via DataService (qui vérifiera la dispo)
                        if (DataService.UpdateReservation(editWin.WorkingCopy))
                        {
                            Logger.Log($"Reservation {res.Id} updated via planning. Reloading data and reapplying filter.");
                            // Recharger TOUTES les données de la base
                            LoadAllData();
                            // Réappliquer le filtre actuel pour mettre à jour _displayedReservations
                            ApplyFilter();
                            // RefreshPlanning est appelé à la fin de ApplyFilter, redessinant la grille
                            MessageBox.Show("Réservation mise à jour.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Echec de la mise à jour. Le créneau est peut-être devenu indisponible ou une autre erreur s'est produite.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Logger.Log($"Update failed for {res.Id} (likely time slot conflict or DB error).");
                        }
                    }
                    catch (Exception ex) { MessageBox.Show($"Erreur lors de la mise à jour : {ex.Message}", "Erreur Technique"); Logger.Log($"Error processing update for {res.Id}: {ex.Message}"); }
                }
                else
                {
                    Logger.Log($"Edit cancelled for reservation {res.Id}.");
                }
            }
        }
        #endregion

        #region Helpers
        // Trouve le Lundi de la semaine contenant la date donnée
        private DateTime GetMonday(DateTime date)
        {
            DayOfWeek currentDay = date.DayOfWeek;
            int daysToSubtract = (int)currentDay - (int)DayOfWeek.Monday;
            if (daysToSubtract < 0) daysToSubtract += 7; // Dimanche (0) devient -6 jours pour aller à Lundi
            return date.AddDays(-daysToSubtract).Date; // Retourne la date sans l'heure
        }

        // Attribue une couleur par clé (ex: nom de salle)
        private Brush GetColorForKey(string key, Dictionary<string, Brush> colorMap)
        {
            if (string.IsNullOrEmpty(key)) key = "default_null_key"; // Clé par défaut si null/vide

            if (!colorMap.ContainsKey(key))
            {
                // Générer une couleur basée sur le hashcode (pour consistance)
                var rnd = new Random(key.GetHashCode());
                byte r = (byte)rnd.Next(180, 245); // Couleurs claires/pastel
                byte g = (byte)rnd.Next(180, 245);
                byte b = (byte)rnd.Next(180, 245);
                colorMap[key] = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            return colorMap[key];
        }
        #endregion
    }
}