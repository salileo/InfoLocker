package com.salilsoftware.InfoLocker.Data;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;

import javax.crypto.Cipher;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class Encryptor
{
	public static void Encrypt(InputStream input, OutputStream output, String password) throws Exception
	{
		Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
	  	byte[] pwdBytes= password.getBytes("utf-8");
		byte[] keyBytes= new byte[16];
	  	int len= pwdBytes.length; 
	  	if (len > keyBytes.length)
	  		len = keyBytes.length;
	  	System.arraycopy(pwdBytes, 0, keyBytes, 0, len);
		SecretKeySpec keySpec = new SecretKeySpec(keyBytes, "AES");
		IvParameterSpec ivSpec = new IvParameterSpec(keyBytes);
		cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);

		byte[] bytes = new byte[1024];
		int blockSize = 1024;
		int bytesRead = 0;
		Boolean finalized = false;
		do
		{
			if (bytesRead > 0)
			{
				if (bytesRead < blockSize)
				{
		            int padding = bytesRead % 16;
		            if (padding > 0)
		                padding = 16 - padding;

		            while (padding > 0)
		            {
		            	bytes[bytesRead] = 0;
		            	bytesRead++;
		                padding--;
		            }
					
					byte[] encodedBytes = cipher.doFinal(bytes, 0, bytesRead);
					output.write(encodedBytes);
					finalized = true;
				}
				else
				{
					byte[] encodedBytes = cipher.update(bytes);
					output.write(encodedBytes);
				}
			}
			
			bytesRead = input.read(bytes);
		}
		while(bytesRead != -1);
		
		if (!finalized)
		{
			byte[] encodedBytes = cipher.doFinal();
			output.write(encodedBytes);
		}

		input.close();
		output.flush();
		output.close();
	}

	public static ByteArrayOutputStream Decrypt(InputStream input, String password) throws Exception
	{
		ByteArrayOutputStream output = new ByteArrayOutputStream();
		
		Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
	  	byte[] pwdBytes= password.getBytes("utf-8");
		byte[] keyBytes= new byte[16];
	  	int len= pwdBytes.length; 
	  	if (len > keyBytes.length)
	  		len = keyBytes.length;
	  	System.arraycopy(pwdBytes, 0, keyBytes, 0, len);
		SecretKeySpec keySpec = new SecretKeySpec(keyBytes, "AES");
		IvParameterSpec ivSpec = new IvParameterSpec(keyBytes);
	    cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
		
		byte[] bytes = new byte[1024];
		int blockSize = 1024;
		int bytesRead = 0;
		Boolean finalized = false;
		do
		{
			if (bytesRead > 0)
			{
				if (bytesRead < blockSize)
				{
					byte[] decodedBytes = cipher.doFinal(bytes, 0, bytesRead);
					output.write(decodedBytes);
					finalized = true;
				}
				else
				{
					byte[] decodedBytes = cipher.update(bytes);
					output.write(decodedBytes);
				}
			}
			
			bytesRead = input.read(bytes);
		}
		while(bytesRead != -1);

		if (!finalized)
		{
			byte[] decodedBytes = cipher.doFinal();
			output.write(decodedBytes);
		}

		input.close();
		output.flush();
		
		return output;
	}
}
