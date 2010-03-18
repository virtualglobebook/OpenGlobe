#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the Boost Software License, Version 1.0.
// See License.txt or http://www.boost.org/LICENSE_1_0.txt.
//
#endregion

using MiniGlobe.Core;
using MiniGlobe.Core.Geometry;

namespace MiniGlobe.Examples.Chapter7
{
    public static class Cities
    {
        public static Vector3D[] International(Ellipsoid globeShape)
        {
            //
            // From http://www.infoplease.com/ipa/A0001769.html
            //
            return new Vector3D[] 
            { 
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-2.9), Trig.ToRadians(57.9), 0)), // Aberdeen, Scotland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(138.36), Trig.ToRadians(-34.55), 0)), // Adelaide, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(3.0), Trig.ToRadians(36.50), 0)), // Algiers, Algeria
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(4.53), Trig.ToRadians(52.22), 0)), // Amsterdam, Netherlands
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(32.55), Trig.ToRadians(39.55), 0)), // Ankara, Turkey
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-57.40), Trig.ToRadians(-25.15), 0)), // Asunción, Paraguay
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(23.43), Trig.ToRadians(37.58), 0)), // Athens, Greece
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(174.45), Trig.ToRadians(-36.52), 0)), // Auckland, New Zealand
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(100.30), Trig.ToRadians(13.45), 0)), // Bangkok, Thailand
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(2.9), Trig.ToRadians(41.23), 0)), // Barcelona, Spain
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(116.25), Trig.ToRadians(9.55), 0)), // Beijing, China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-48.29), Trig.ToRadians(-1.28), 0)), // Belém, Brazil
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-5.56), Trig.ToRadians(54.37), 0)), // Belfast, Northern Ireland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(20.32), Trig.ToRadians(44.52), 0)), // Belgrade, Serbia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(13.25), Trig.ToRadians(52.30), 0)), // Berlin, Germany
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-1.55), Trig.ToRadians(52.25), 0)), // Birmingham, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-74.15), Trig.ToRadians(4.32), 0)), // Bogotá, Colombia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(72.48), Trig.ToRadians(19.0), 0)), // Bombay, India
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-0.31), Trig.ToRadians(44.50), 0)), // Bordeaux, France
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(8.49), Trig.ToRadians(53.5), 0)), // Bremen, Germany
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(153.8), Trig.ToRadians(-27.29), 0)), // Brisbane, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-2.35), Trig.ToRadians(51.28), 0)), // Bristol, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(4.22), Trig.ToRadians(50.52), 0)), // Brussels, Belgium
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(26.7), Trig.ToRadians(44.25), 0)), // Bucharest, Romania
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(19.5), Trig.ToRadians(47.30), 0)), // Budapest, Hungary
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-58.22), Trig.ToRadians(-34.35), 0)), // Buenos Aires, Argentina
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(31.21), Trig.ToRadians(30.2), 0)), // Cairo, Egypt
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(88.24), Trig.ToRadians(22.34), 0)), // Calcutta, India
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(113.15), Trig.ToRadians(23.7), 0)), // Canton, China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(18.22), Trig.ToRadians(-33.55), 0)), // Cape Town, South Africa
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-67.2), Trig.ToRadians(10.28), 0)), // Caracas, Venezuela
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-52.18), Trig.ToRadians(4.49), 0)), // Cayenne, French Guiana
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-106.5), Trig.ToRadians(28.37), 0)), // Chihuahua, Mexico
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(106.34), Trig.ToRadians(29.46), 0)), // Chongqing, China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(12.34), Trig.ToRadians(55.40), 0)), // Copenhagen, Denmark
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-64.10), Trig.ToRadians(-31.28), 0)), // Córdoba, Argentina
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-17.28), Trig.ToRadians(14.40), 0)), // Dakar, Senegal
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(130.51), Trig.ToRadians(-12.28), 0)), // Darwin, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(43.3), Trig.ToRadians(11.30), 0)), // Djibouti, Djibouti
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-6.15), Trig.ToRadians(53.20), 0)), // Dublin, Ireland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(30.53), Trig.ToRadians(-29.53), 0)), // Durban, South Africa
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-3.10), Trig.ToRadians(55.55), 0)), // Edinburgh, Scotland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(8.41), Trig.ToRadians(50.7), 0)), // Frankfurt, Germany
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-58.15), Trig.ToRadians(6.45), 0)), // Georgetown, Guyana
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-4.15), Trig.ToRadians(55.50), 0)), // Glasgow, Scotland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-90.31), Trig.ToRadians(14.37), 0)), // Guatemala City, Guatemala
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-79.56), Trig.ToRadians(-2.10), 0)), // Guayaquil, Ecuador
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(10.2), Trig.ToRadians(53.33), 0)), // Hamburg, Germany
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(23.38), Trig.ToRadians(70.38), 0)), // Hammerfest, Norway
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-82.23), Trig.ToRadians(23.8), 0)), // Havana, Cuba
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(25.0), Trig.ToRadians(60.10), 0)), // Helsinki, Finland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(147.19), Trig.ToRadians(-42.52), 0)), // Hobart, Tasmania
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(114.11), Trig.ToRadians(22.20), 0)), // Hong Kong, China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-70.7), Trig.ToRadians(-20.10), 0)), // Iquique, Chile
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(104.20), Trig.ToRadians(52.30), 0)), // Irkutsk, Russia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(106.48), Trig.ToRadians(-6.16), 0)), // Jakarta, Indonesia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(28.4), Trig.ToRadians(-26.12), 0)), // Johannesburg, South Africa
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-76.49), Trig.ToRadians(17.59), 0)), // Kingston, Jamaica
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(15.17), Trig.ToRadians(-4.18), 0)), // Kinshasa, Congo
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(101.42), Trig.ToRadians(3.8), 0)), // Kuala Lumpur, Malaysia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-68.22), Trig.ToRadians(-16.27), 0)), // La Paz, Bolivia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-1.30), Trig.ToRadians(53.45), 0)), // Leeds, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-77.2), Trig.ToRadians(-12.0), 0)), // Lima, Peru
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-9.9), Trig.ToRadians(38.44), 0)), // Lisbon, Portugal
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-3.0), Trig.ToRadians(53.25), 0)), // Liverpool, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-0.5), Trig.ToRadians(51.32), 0)), // London, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(4.50), Trig.ToRadians(45.45), 0)), // Lyons, France
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-3.42), Trig.ToRadians(40.26), 0)), // Madrid, Spain
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-2.15), Trig.ToRadians(53.30), 0)), // Manchester, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(120.57), Trig.ToRadians(4.35), 0)), // Manila, Philippines
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(5.20), Trig.ToRadians(43.20), 0)), // Marseilles, France
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-106.25), Trig.ToRadians(23.12), 0)), // Mazatlán, Mexico
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(39.45), Trig.ToRadians(21.29), 0)), // Mecca, Saudi Arabia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(144.58), Trig.ToRadians(-37.47), 0)), // Melbourne, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-99.7), Trig.ToRadians(19.26), 0)), // Mexico City, Mexico
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(9.10), Trig.ToRadians(45.27), 0)), // Milan, Italy
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-56.10), Trig.ToRadians(-34.53), 0)), // Montevideo, Uruguay
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(37.36), Trig.ToRadians(55.45), 0)), // Moscow, Russia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(11.35), Trig.ToRadians(48.8), 0)), // Munich, Germany
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(129.57), Trig.ToRadians(32.48), 0)), // Nagasaki, Japan
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(136.56), Trig.ToRadians(35.7), 0)), // Nagoya, Japan
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(36.55), Trig.ToRadians(-1.25), 0)), // Nairobi, Kenya
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(118.53), Trig.ToRadians(32.3), 0)), // Nanjing (Nanking), China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(14.15), Trig.ToRadians(40.50), 0)), // Naples, Italy
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(77.12), Trig.ToRadians(28.35), 0)), // New Delhi, India
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-1.37), Trig.ToRadians(54.58), 0)), // Newcastle-on-Tyne, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(30.48), Trig.ToRadians(46.27), 0)), // Odessa, Ukraine
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(135.30), Trig.ToRadians(34.32), 0)), // Osaka, Japan
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(10.42), Trig.ToRadians(59.57), 0)), // Oslo, Norway
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-79.32), Trig.ToRadians(8.58), 0)), // Panama City, Panama
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-55.15), Trig.ToRadians(5.45), 0)), // Paramaribo, Suriname
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(2.20), Trig.ToRadians(48.48), 0)), // Paris, France
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(115.52), Trig.ToRadians(-31.57), 0)), // Perth, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-4.5), Trig.ToRadians(50.25), 0)), // Plymouth, England
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(147.8), Trig.ToRadians(-9.25), 0)), // Port Moresby, Papua New Guinea
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(14.26), Trig.ToRadians(50.5), 0)), // Prague, Czech Republic
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(96.0), Trig.ToRadians(16.50), 0)), // Rangoon, Myanmar
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-21.58), Trig.ToRadians(64.4), 0)), // Reykjavík, Iceland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-43.12), Trig.ToRadians(-22.57), 0)), // Rio de Janeiro, Brazil
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(12.27), Trig.ToRadians(41.54), 0)), // Rome, Italy
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-38.27), Trig.ToRadians(-12.56), 0)), // Salvador, Brazil
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-70.45), Trig.ToRadians(-33.28), 0)), // Santiago, Chile
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(30.18), Trig.ToRadians(59.56), 0)), // St. Petersburg, Russia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-46.31), Trig.ToRadians(-23.31), 0)), // São Paulo, Brazil
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(121.28), Trig.ToRadians(31.10), 0)), // Shanghai, China
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(103.55), Trig.ToRadians(1.14), 0)), // Singapore, Singapore
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(23.20), Trig.ToRadians(42.40), 0)), // Sofia, Bulgaria
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(18.3), Trig.ToRadians(59.17), 0)), // Stockholm, Sweden
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(151.0), Trig.ToRadians(-34.0), 0)), // Sydney, Australia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(47.33), Trig.ToRadians(-18.50), 0)), // Tananarive, Madagascar
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(51.45), Trig.ToRadians(35.45), 0)), // Teheran, Iran
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(139.45), Trig.ToRadians(35.40), 0)), // Tokyo, Japan
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(13.12), Trig.ToRadians(32.57), 0)), // Tripoli, Libya
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(12.20), Trig.ToRadians(45.26), 0)), // Venice, Italy
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-96.10), Trig.ToRadians(19.10), 0)), // Veracruz, Mexico
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(16.20), Trig.ToRadians(48.14), 0)), // Vienna, Austria
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(132.0), Trig.ToRadians(43.10), 0)), // Vladivostok, Russia
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(21.0), Trig.ToRadians(52.14), 0)), // Warsaw, Poland
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(174.47), Trig.ToRadians(-41.17), 0)), // Wellington, New Zealand
                globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(8.31), Trig.ToRadians(47.21), 0)), // Zürich, Switzerland
            };
        }
   }
}