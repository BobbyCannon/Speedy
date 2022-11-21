#region References

using System.ComponentModel.DataAnnotations;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea;

public enum NmeaMessagePrefix
{
	[Display(ShortName = "--", Name = "Unknown")]
	Unknown = 0,

	/// <summary>
	/// GL - GLONASS Receiver
	/// </summary>
	[Display(ShortName = "GL", Name = "GLONASS Receiver")]
	GlonassReceiver = 1,

	/// <summary>
	/// GP - Global Positioning System (GPS)
	/// </summary>
	[Display(ShortName = "GP", Name = "Global Positioning System (GPS)")]
	GlobalPositioningSystem = 2,

	/// <summary>
	/// AI - Automatic Identification System
	/// </summary>
	[Display(ShortName = "AI", Name = "Automatic Identification System")]
	AutomaticIdentificationSystem = 3,

	/// <summary>
	/// CD - Digital Selective Calling (DSC)
	/// </summary>
	[Display(ShortName = "CD", Name = "Digital Selective Calling (DSC)")]
	DigitalSelectiveCalling = 4,

	/// <summary>
	/// CR - Data Receiver
	/// </summary>
	[Display(ShortName = "CR", Name = "Data Receiver")]
	DataReceiver = 5,

	/// <summary>
	/// CS - Satellite
	/// </summary>
	[Display(ShortName = "CS", Name = "Satellite")]
	Satellite = 6,

	/// <summary>
	/// CT - Radio-Telephone (MF/HF)
	/// </summary>
	[Display(ShortName = "CT", Name = "Radio-Telephone (MF/HF)")]
	RadioTelephoneMfHf = 7,

	/// <summary>
	/// CV - Radio-Telephone (VHF)
	/// </summary>
	[Display(ShortName = "CV", Name = "Radio-Telephone (VHF)")]
	RadioTelephoneVhf = 8,

	/// <summary>
	/// CX - Scanning Receiver
	/// </summary>
	[Display(ShortName = "CX", Name = "Scanning Receiver")]
	ScanningReceiver = 9,

	/// <summary>
	/// DE - DECCA Navigator
	/// </summary>
	[Display(ShortName = "DE", Name = "DECCA Navigator")]
	DeccaNavigator = 10,

	/// <summary>
	/// DF - Direction Finder
	/// </summary>
	[Display(ShortName = "DF", Name = "Direction Finder")]
	DirectionFinder = 11,

	/// <summary>
	/// EC - Electronic Chart System (ECS)
	/// </summary>
	[Display(ShortName = "EC", Name = "Electronic Chart System (ECS)")]
	ElectronicChartSystem = 12,

	/// <summary>
	/// EI - Electronic Chart Display &amp; Information System (ECDIS)
	/// </summary>
	[Display(ShortName = "EI", Name = "Electronic Chart Display & Information System (ECDIS)")]
	ElectronicChartDisplayAndInformationSystem = 13,

	/// <summary>
	/// EP - Emergency Position Indicating Beacon (EPIRB)
	/// </summary>
	[Display(ShortName = "EP", Name = "Emergency Position Indicating Beacon (EPIRB)")]
	EmergencyPositionIndicatingBeacon = 14,

	/// <summary>
	/// ER - Engine Room Monitoring Systems
	/// </summary>
	[Display(ShortName = "ER", Name = "Engine Room Monitoring Systems")]
	EngineRoomMonitoringSystems = 15,

	/// <summary>
	/// GN - Global Navigation Satellite System (GNSS)
	/// </summary>
	[Display(ShortName = "GN", Name = "Global Navigation Satellite System (GNSS)")]
	GlobalNavigationSatelliteSystem = 16,

	/// <summary>
	/// HC - HEADING SENSORS: Compass, Magnetic
	/// </summary>
	[Display(ShortName = "HC", Name = "HEADING SENSORS: Compass, Magnetic")]
	HeadingSensors = 17,

	/// <summary>
	/// HE - Gyro, North Seeking
	/// </summary>
	[Display(ShortName = "HE", Name = "Gyro, North Seeking")]
	GyroNorthSeeking = 18,

	/// <summary>
	/// HN - Gyro, Non-North Seeking
	/// </summary>
	[Display(ShortName = "HN", Name = "Gyro, Non-North Seeking")]
	GyroNonNorthSeeking = 19,

	/// <summary>
	/// II - Integrated Instrumentation
	/// </summary>
	[Display(ShortName = "II", Name = "Integrated Instrumentation")]
	IntegratedInstrumentation = 20,

	/// <summary>
	/// IN - Integrated Navigation
	/// </summary>
	[Display(ShortName = "IN", Name = "Integrated Navigation")]
	IntegratedNavigation = 21,

	/// <summary>
	/// LC - Loran C
	/// </summary>
	[Display(ShortName = "LC", Name = "Loran C")]
	LoranC = 22,

	/// <summary>
	/// RA - Radar and/or Radar Plotting
	/// </summary>
	[Display(ShortName = "RA", Name = "Radar and/or Radar Plotting")]
	RadarAndOrRadarPlotting = 23,

	/// <summary>
	/// SD - Sounder, depth
	/// </summary>
	[Display(ShortName = "SD", Name = "Sounder, depth")]
	SounderDepth = 24,

	/// <summary>
	/// SN - Electronic Positioning System, other/general
	/// </summary>
	[Display(ShortName = "SN", Name = "Electronic Positioning System, other/general")]
	ElectronicPositioningSystem = 25,

	/// <summary>
	/// SS - Sounder, scanning
	/// </summary>
	[Display(ShortName = "SS", Name = "Sounder, scanning")]
	SounderScanning = 26,

	/// <summary>
	/// TI - Turn Rate Indicator
	/// </summary>
	[Display(ShortName = "TI", Name = "Turn Rate Indicator")]
	TurnRateIndicator = 27,

	/// <summary>
	/// VD - VELOCITY SENSORS: Doppler, other/general
	/// </summary>
	[Display(ShortName = "VD", Name = "VELOCITY SENSORS: Doppler, other/general")]
	VelocitySensors = 28,

	/// <summary>
	/// VM - Speed Log, Water, Magnetic
	/// </summary>
	[Display(ShortName = "VM", Name = "Speed Log, Water, Magnetic")]
	SpeedLogWaterMagnetic = 29,

	/// <summary>
	/// VW - Speed Log, Water, Mechanical
	/// </summary>
	[Display(ShortName = "VW", Name = "Speed Log, Water, Mechanical")]
	SpeedLogWaterMechanical = 30,

	/// <summary>
	/// VR - Voyage Data Recorder
	/// </summary>
	[Display(ShortName = "VR", Name = "Voyage Data Recorder")]
	VoyageDataRecorder = 31,

	/// <summary>
	/// YX - Transducer
	/// </summary>
	[Display(ShortName = "YX", Name = "Transducer")]
	Transducer = 32,

	/// <summary>
	/// ZA - TIMEKEEPERS, TIME/DATE: Atomic Clock
	/// </summary>
	[Display(ShortName = "ZA", Name = "TIMEKEEPERS, TIME/DATE: Atomic Clock")]
	TimeKeepers = 33,

	/// <summary>
	/// ZC - Chronometer
	/// </summary>
	[Display(ShortName = "ZC", Name = "Chronometer")]
	Chronometer = 34,

	/// <summary>
	/// ZQ - Quartz
	/// </summary>
	[Display(ShortName = "ZQ", Name = "Quartz")]
	Quartz = 35,

	/// <summary>
	/// ZV - Radio Update
	/// </summary>
	[Display(ShortName = "ZV", Name = "Radio Update")]
	RadioUpdate = 36,

	/// <summary>
	/// WI - Weather Instruments
	/// </summary>
	[Display(ShortName = "WI", Name = "Weather Instruments")]
	WeatherInstruments = 37
}