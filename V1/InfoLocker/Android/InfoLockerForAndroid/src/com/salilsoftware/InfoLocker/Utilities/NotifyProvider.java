package com.salilsoftware.InfoLocker.Utilities;

import java.util.LinkedList;


public class NotifyProvider
{
	public LinkedList<NotifyConsumer> Listners;
	
	public NotifyProvider()
	{
		Listners = new LinkedList<NotifyConsumer>();
	}
	
	public void NotifyPropertyChanged(String propName)
	{
		for (NotifyConsumer consumer : Listners)
			consumer.HandlePropertyChange(this, propName);
	}
}
