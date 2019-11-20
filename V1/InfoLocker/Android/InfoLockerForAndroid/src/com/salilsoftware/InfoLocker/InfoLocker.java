package com.salilsoftware.InfoLocker;

import com.salilsoftware.InfoLocker.Data.Node_Note;
import com.salilsoftware.InfoLocker.Data.StorageFile;

import android.app.Activity;
import android.os.Bundle;

public class InfoLocker extends Activity {
    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
     
        try
        {
			StorageFile file = new StorageFile("/sdcard/salil.stg");
			file.UnLock("sumi1234");
			
			Node_Note newNote = new Node_Note();
			newNote.Name("test");
			newNote.Content("hi there");
			file.RootNode().AddNode(newNote);
			
			file.Close(true);
		}
        catch (Exception e)
        {
			e.printStackTrace();
		}
    }
}