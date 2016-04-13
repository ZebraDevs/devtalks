package com.symbol.devtalks_scanandpair;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;

import android.util.Log;
import android.widget.TextView;

import com.symbol.emdk.EMDKManager;
import com.symbol.emdk.EMDKResults;
import com.symbol.emdk.EMDKManager.EMDKListener;
import com.symbol.emdk.EMDKManager.FEATURE_TYPE;
import com.symbol.emdk.scanandpair.ScanAndPairConfig;
import com.symbol.emdk.scanandpair.ScanAndPairConfig.ScanDataType;
import com.symbol.emdk.scanandpair.ScanAndPairConfig.TriggerType;
import com.symbol.emdk.scanandpair.ScanAndPairException;
import com.symbol.emdk.scanandpair.StatusData;
import com.symbol.emdk.scanandpair.ScanAndPairConfig.NotificationType;
import com.symbol.emdk.scanandpair.ScanAndPairResults;
import com.symbol.emdk.scanandpair.ScanAndPairManager;

public class MainActivity extends AppCompatActivity implements EMDKListener,
        ScanAndPairManager.StatusListener {

    ScanAndPairManager.StatusListener statusCallbackObj = this;
    private EMDKManager emdkManager;
    private ScanAndPairManager scanAndPairMgr;

    TextView tv1 = null;

    StringBuilder sb = new StringBuilder();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        tv1 = (TextView) findViewById(R.id.tv1);

        EMDKResults results = EMDKManager.getEMDKManager(getApplicationContext(), this);

        // Check the return status of getEMDKManager ()
        if (results.statusCode == EMDKResults.STATUS_CODE.SUCCESS) {
            UpdateUI("Successfuly requested EMDKManager Object");

        } else {
            UpdateUI("Failure while requesting EMDKManager Object: " + results.getStatusString());
        }


    }


    void UpdateUI(String message) {
        final String text = message;

        runOnUiThread(new Runnable() {
            public void run() {
                sb.insert(0, text + "\n----------------------------------------\n");
                tv1.setText(sb.toString());

            }
        });
    }

    @Override
    public void onOpened(EMDKManager emdkManager) {
        this.emdkManager = emdkManager;


        if (scanAndPairMgr == null) {
            scanAndPairMgr = (ScanAndPairManager) emdkManager.getInstance(FEATURE_TYPE.SCANANDPAIR);

            if (scanAndPairMgr != null) {
                try {
                    scanAndPairMgr.addStatusListener(statusCallbackObj);

                    //Pair with BlueTooth Printer
                    scanAndPairMgr.config.notificationType = NotificationType.BEEPER;

                    //scanAndPairMgr.config.scanInfo.scanTimeout = 5000;
                    //scanAndPairMgr.config.scanInfo.deviceIdentifier = ScanAndPairConfig.DeviceIdentifier.DEFAULT;
                    scanAndPairMgr.config.scanInfo.triggerType = TriggerType.SOFT;
                    //scanAndPairMgr.config.alwaysScan = true;

                    scanAndPairMgr.config.scanInfo.scanDataType = ScanDataType.MAC_ADDRESS;

                    ScanAndPairResults resultCode = scanAndPairMgr.scanAndPair("0000");
                    //ScanAndPairResults resultCode = scanAndPairMgr.scanAndUnpair();


                    //Pair with Payment Module by Name
//                    scanAndPairMgr.config.notificationType = NotificationType.BEEPER;
//
//                    scanAndPairMgr.config.alwaysScan = false;
//
//                    scanAndPairMgr.config.bluetoothInfo.deviceName = "MPOS-64475985";
//
//                    ScanAndPairResults resultCode = scanAndPairMgr.scanAndPair("0000");
                    //ScanAndPairResults resultCode = scanAndPairMgr.scanAndUnpair();



                    if (!resultCode.equals(ScanAndPairResults.SUCCESS))
                        UpdateUI(resultCode.toString() + "\n\n");


                } catch (ScanAndPairException e) {
                    e.printStackTrace();
                }
            }
        }


    }

    @Override
    public void onClosed() {

    }

    @Override
    public void onStatus(StatusData statusData) {

        switch (statusData.getState()) {
            case WAITING:
                UpdateUI("Waiting for trigger press to scan the barcode");
                break;

            case SCANNING:
                UpdateUI("Scanner Beam is on, aim at the barcode.");
                break;

            case DISCOVERING:
                UpdateUI("Discovering for the Bluetooth device");
                break;

            case PAIRED:
                UpdateUI("Bluetooth device is paired successfully");
                break;

            case UNPAIRED:
                UpdateUI("Bluetooth device is un-paired successfully");
                break;

            default:
            case ERROR:
                UpdateUI(statusData.getState().toString() + ": " + statusData.getResult());
                break;
        }

    }
}
