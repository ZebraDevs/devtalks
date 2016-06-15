package com.symbol.zebraandnonzebra;


import android.os.Build;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.widget.Toast;

import com.symbol.emdk.EMDKManager;
import com.symbol.emdk.EMDKResults;


public class MainActivity extends AppCompatActivity {
    EMDKWrapper emdkWrapper = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        if (Build.MANUFACTURER.contains("Zebra Technologies") || Build.MANUFACTURER.contains("Motorola Solutions")) {

            emdkWrapper = new EMDKWrapper();
            emdkWrapper.getEMDKManager();

        } else {
            Toast.makeText(MainActivity.this, "Non-Zebra", Toast.LENGTH_SHORT).show();
        }

    }


    public class EMDKWrapper implements EMDKManager.EMDKListener {
        EMDKManager emdkManager = null;

        void getEMDKManager() {
            EMDKResults results = EMDKManager.getEMDKManager(getApplicationContext(), this);

            if (results.statusCode != EMDKResults.STATUS_CODE.SUCCESS) {
                //Failed to request the EMDKManager
            }
        }


        void release() {
            if (emdkManager != null) {
                emdkManager.release();
                emdkManager = null;
            }
        }

        @Override
        public void onOpened(EMDKManager emdkManager) {
            this.emdkManager = emdkManager;

            Toast.makeText(MainActivity.this, "EMDK ready!", Toast.LENGTH_SHORT).show();

        }


        @Override
        public void onClosed() {
            if (emdkManager != null) {
                emdkManager.release();
                emdkManager = null;
            }
        }
    }


    @Override
    protected void onDestroy() {
        super.onDestroy();

        if(emdkWrapper != null){
            emdkWrapper.release();
        }
    }
}
