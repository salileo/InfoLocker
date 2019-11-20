package com.salilsoftware.InfoLocker.Utilities;

public class StringUtils
{
	public static Boolean IsNullOrEmpty(String str)
	{
		if ((str == null) || (str.length() <= 0))
			return true;
		else
			return false;
	}
	
	public static Boolean Equals(String str1, String str2)
	{
		if (str1 == str2)
			return true;
		
		if (str1 != null)
			return str1.equals(str2);
		
		return false;
	}

	public static Boolean EqualsNoCase(String str1, String str2)
	{
		if (str1 == str2)
			return true;
		
		if (str1 != null)
			return (str1.compareToIgnoreCase(str2) == 0);
		
		return false;
	}
}
