package com.salilsoftware.InfoLocker.Utilities;

public interface NotifyConsumer
{
	public abstract void HandlePropertyChange(NotifyProvider source, String propName);
}
