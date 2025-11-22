namespace BluebirdCore.Models
{
    /// <summary>
    /// Configuration settings for school branding and customization
    /// </summary>
    public class SchoolSettings
    {
        public const string SectionName = "School";

        /// <summary>
        /// School name to display on report cards and documents
        /// </summary>
        public string Name { get; set; } = "School Management System";

        /// <summary>
        /// School email address for contact information
        /// </summary>
        public string Email { get; set; } = "info@school.com";

        /// <summary>
        /// School website URL
        /// </summary>
        public string Website { get; set; } = "www.school.com";

        /// <summary>
        /// Path to the main school logo (relative to Media folder)
        /// </summary>
        public string LogoPath { get; set; } = "./Media/logo.png";

        /// <summary>
        /// Path to the watermark logo (relative to Media folder)
        /// </summary>
        public string WatermarkLogoPath { get; set; } = "./Media/logo-wm.png";
    }
}

