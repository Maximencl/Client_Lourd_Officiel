using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ReservationSalles.Services
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Hashage SHA256 d'une chaîne UTF-8.
        /// </summary>
        /// <param name="rawData">La chaîne de caractères à hasher (mot de passe en clair).</param>
        /// <returns>Le hash SHA256 représenté sous forme de chaîne hexadécimale minuscule.</returns>
        public static string ComputeSha256Hash(string rawData)
        {
            // Vérifier si l'entrée est nulle ou vide pour éviter une exception
            if (string.IsNullOrEmpty(rawData))
            {
                // Il serait peut-être préférable de lancer une ArgumentNullException ici
                // return string.Empty; 
                throw new ArgumentNullException(nameof(rawData), "Le mot de passe ne peut pas être nul ou vide.");
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Calculer le hash des bytes UTF-8 de la chaîne
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convertir le tableau de bytes en une chaîne hexadécimale
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // "x2" pour format hexadécimal minuscule sur 2 caractères
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Vérifie qu'un mot de passe répond à la complexité:
        /// Au moins 12 caractères, au moins 1 majuscule, au moins 1 minuscule, au moins 2 chiffres, au moins 1 caractère spécial.
        /// </summary>
        /// <param name="password">Le mot de passe à vérifier.</param>
        /// <returns>True si le mot de passe est valide, sinon False.</returns>
        public static bool IsPasswordValid(string password)
        {
            // Vérification de base
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // Regex qui impose la règle demandée
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=(?:.*\d){2})(?=.*[^a-zA-Z0-9]).{12,}$";

            return Regex.IsMatch(password, pattern);
        }

        /// <summary>
        /// Vérifie si un mot de passe fourni (en clair) correspond à un hash stocké.
        /// IMPORTANT : Cette méthode est fonctionnelle MAIS N'EST PAS SÉCURISÉE contre les attaques par tables arc-en-ciel.
        /// Il est fortement recommandé d'implémenter le salage des mots de passe.
        /// </summary>
        /// <param name="passwordEntered">Le mot de passe entré par l'utilisateur (clair).</param>
        /// <param name="storedHash">Le hash du mot de passe stocké dans la base de données (chaîne hexadécimale).</param>
        /// <returns>True si les mots de passe correspondent (après hachage), sinon False.</returns>
        internal static bool VerifyPassword(string passwordEntered, string storedHash)
        {
            // Gérer les cas où l'un ou l'autre est null/vide pour éviter les erreurs
            // Note : Si ComputeSha256Hash lance une exception sur null/vide, ce check n'est plus nécessaire
            // pour passwordEntered, mais reste utile pour storedHash.
            if (string.IsNullOrEmpty(passwordEntered) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            // Hasher le mot de passe entré avec la même méthode que lors de la création/mise à jour
            string hashOfPasswordEntered = ComputeSha256Hash(passwordEntered);

            // Comparer les deux hashes. Utiliser StringComparer.Ordinal est requis pour une
            // comparaison exacte des chaînes hexadécimales (qui sont sensibles à la casse par nature,
            // même si notre fonction ComputeSha256Hash produit toujours des minuscules).
            return StringComparer.Ordinal.Equals(hashOfPasswordEntered, storedHash);
        }

        // --- AMÉLIORATION DE SÉCURITÉ RECOMMANDÉE : Salage ---
        // Pour une sécurité accrue, vous devriez :
        // 1. Générer un "sel" (salt) aléatoire unique pour chaque utilisateur lors de la création.
        // 2. Stocker ce sel avec l'utilisateur dans la base de données.
        // 3. Modifier ComputeSha256Hash pour accepter le mot de passe ET le sel, et hasher la combinaison (ex: password + salt).
        // 4. Modifier VerifyPassword pour récupérer le sel de l'utilisateur, hasher le mot de passe entré AVEC ce sel,
        //    puis comparer le résultat avec le hash stocké.
        // Exemple de structure (simplifié) :
        // public static (string hash, string salt) HashPasswordWithSalt(string password) { ... génère sel, hashe password+sel ... }
        // public static bool VerifyPasswordWithSalt(string enteredPassword, string storedHash, string storedSalt) { ... hashe enteredPassword+storedSalt, compare ... }
    }
}