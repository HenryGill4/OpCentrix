using System;
using System.Linq;
using OpCentrix.Models;
using System.Text.RegularExpressions;

namespace OpCentrix.Services
{
    internal static class TaskTriggerHelpers
    {
        // Normalizes material strings to broad families used in task triggering
        public static string NormalizeMaterial(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Any";
            raw = raw.Trim();
            var lower = raw.ToLowerInvariant();

            // Fast exact tokens first
            switch (lower)
            {
                case "inc":
                case "inco":
                case "inconel":
                case "in718":
                case "in625":
                    return "Inconel";
                case "ti":
                case "ti64":
                case "ti-6al-4v":
                case "titanium":
                case "ti-6al-4v grade 5":
                case "ti-6al-4v grade 23":
                    return "Titanium";
                case "pa":
                case "pa12":
                case "pa-12":
                    return "PA12";
            }

            // Pattern / substring based mapping (covers friendly names and variants)
            if (lower.Contains("inconel") || lower.Contains("in718") || lower.Contains("in-718") || lower.Contains("718") && lower.Contains("in") || lower.Contains("in625") || lower.Contains("in-625"))
                return "Inconel";

            if (lower.Contains("ti-6al") || lower.Contains("ti64") || (lower.Contains("ti") && lower.Contains("grade") && (lower.Contains("5") || lower.Contains("23"))) || lower.Contains("titanium"))
                return "Titanium";

            if (lower.Contains("pa12") || lower.Contains("pa-12"))
                return "PA12";

            if (lower.Contains("316l") || lower.Contains("stainless"))
                return "Stainless"; // optional broader family for future tasks

            // Aluminum family examples
            if (lower.Contains("alsi10mg") || lower.Contains("al-si10") || lower.Contains("al si10"))
                return "AlSi10Mg";

            return raw; // fallback to original text (case preserved as passed in)
        }

        // Resolves the effective material for a build (snapshot > part.SlsMaterial > part.Material)
        public static string ResolveEffectiveMaterial(BuildJob b)
        {
            var mat = b.Material;
            if (string.IsNullOrWhiteSpace(mat))
            {
                if (b.Part != null)
                    mat = b.Part.SlsMaterial ?? b.Part.Material;
            }
            return mat ?? "Any";
        }
    }
}
