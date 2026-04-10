namespace SCG.Rules.Infrastructure.Persistence.Seeds;

public static class MasterNationalityList
{
    public static IReadOnlyList<MasterNationalityItem> GetAll() => Items;

    private static readonly List<MasterNationalityItem> Items =
    [
        new("SY", "Syria", "سوريا"),
        new("IQ", "Iraq", "العراق"),
        new("LY", "Libya", "ليبيا"),
        new("YE", "Yemen", "اليمن"),
        new("SD", "Sudan", "السودان"),
        new("IR", "Iran", "إيران"),
        new("AF", "Afghanistan", "أفغانستان"),
        new("SO", "Somalia", "الصومال"),
        new("PK", "Pakistan", "باكستان"),
        new("LB", "Lebanon", "لبنان"),
        new("JO", "Jordan", "الأردن"),
        new("PS", "Palestine", "فلسطين"),
        new("ET", "Ethiopia", "إثيوبيا"),
        new("ER", "Eritrea", "إريتريا"),
        new("IN", "India", "الهند"),
        new("BD", "Bangladesh", "بنغلاديش"),
        new("NP", "Nepal", "نيبال"),
        new("LK", "Sri Lanka", "سريلانكا"),
        new("PH", "Philippines", "الفلبين"),
        new("TR", "Turkey", "تركيا"),
        new("SA", "Saudi Arabia", "السعودية"),
        new("AE", "UAE", "الإمارات"),
        new("KW", "Kuwait", "الكويت"),
        new("QA", "Qatar", "قطر"),
        new("BH", "Bahrain", "البحرين"),
        new("OM", "Oman", "عُمان"),
        new("EG", "Egypt", "مصر"),
        new("MA", "Morocco", "المغرب"),
        new("DZ", "Algeria", "الجزائر"),
        new("TN", "Tunisia", "تونس")
    ];
}

public sealed record MasterNationalityItem(string Code, string NameEn, string NameAr);
