package com.example.skipassscanner

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.collection.emptyLongSet
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.VIEW_MODEL_STORE_OWNER_KEY
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.Response
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.budiyev.android.codescanner.AutoFocusMode
import com.budiyev.android.codescanner.CodeScanner
import com.budiyev.android.codescanner.CodeScannerView
import com.budiyev.android.codescanner.DecodeCallback
import com.budiyev.android.codescanner.ErrorCallback
import com.budiyev.android.codescanner.ScanMode
import com.example.skipassscanner.ui.theme.SkipassScannerTheme
import com.google.zxing.BarcodeFormat
import org.json.JSONObject
import kotlin.system.exitProcess

// NOTE: to get this to work, run your server with --urls="your_local_ip:port" param and change this to that ip and port
const val ip: String = "192.168.1.19:5168"

class MainActivity : ComponentActivity() {
    private lateinit var scanner: CodeScanner

    private var requestQueue: RequestQueue? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        requestQueue = Volley.newRequestQueue(this)

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED)
        {
            val requestPermissionLauncher =
                registerForActivityResult(
                    ActivityResultContracts.RequestPermission()
                ) { isGranted: Boolean ->
                    if (isGranted)
                    {
                        Toast.makeText(this, "Camera permission granted", Toast.LENGTH_LONG).show()
                        // Permission is granted. Continue the action or workflow in your
                        // app.
                    }
                    else
                    {
                        MainActivity().finish()
                        exitProcess(0)
                        // Explain to the user that the feature is unavailable because the
                        // feature requires a permission that the user has denied. At the
                        // same time, respect the user's decision. Don't link to system
                        // settings in an effort to convince the user to change their
                        // decision.
                    }
                }
            requestPermissionLauncher.launch(Manifest.permission.CAMERA)
        }

        val sv: CodeScannerView = CodeScannerView(this)
        scanner = CodeScanner(applicationContext, sv)
        scanner.camera = CodeScanner.CAMERA_BACK
        scanner.formats = listOf(BarcodeFormat.QR_CODE)
        scanner.autoFocusMode = AutoFocusMode.SAFE
        scanner.scanMode = ScanMode.SINGLE
        scanner.isAutoFocusEnabled = true
        scanner.isFlashEnabled = false

        scanner.decodeCallback = DecodeCallback {
            runOnUiThread {
                Toast.makeText(this, "Scan result: ${it.text}", Toast.LENGTH_LONG).show()
                val request = JsonObjectRequest(
                Request.Method.GET, "http://$ip/api/tickets/verify-mobile/" + it.text, null,
                { response ->
                    if (response.getBoolean("valid"))
                    {
                        var text = ""
                        if(!response.isNull("expiration")) {
                            text += " expires: " + response.getString("expiration")
                        }
                        if(!response.isNull("rideLimit"))
                        {
                            text += " (rides left: ${(response.getInt("rideLimit") - response.getInt("ridesTaken"))})"
                        }
                        Toast.makeText(this, "Found a valid ticket.$text", Toast.LENGTH_LONG).show()
                        val request2 = StringRequest(Request.Method.PUT, "http://$ip/api/tickets/${response.getInt("ticketId")}/incrementRide",
                            {
                                Toast.makeText(this, "Ticket registered.", Toast.LENGTH_LONG).show()
                            },
                            { error ->
                                Toast.makeText(this, "Failed to register: " + error.message, Toast.LENGTH_LONG).show()
                            })
                        requestQueue?.add(request2)
                    }
                    else {
                        Toast.makeText(this, "Could not find a valid ticket. Message from the server: " + response.getString("message"), Toast.LENGTH_LONG)
                            .show()
                    }
                },
                {
                    error ->
                    Toast.makeText(this, "Something went wrong: " + error.message, Toast.LENGTH_LONG).show()
                })
                requestQueue?.add(request)
            }
        }
        scanner.errorCallback = ErrorCallback { // or ErrorCallback.SUPPRESS
            runOnUiThread {
                Toast.makeText(this, "Camera initialization error: ${it.message}",
                    Toast.LENGTH_LONG).show()
            }
        }

        sv.setOnClickListener {
            scanner.startPreview()
        }

        setContent {
            SkipassScannerTheme {
                /*Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Greeting(
                        name = "Android",
                        modifier = Modifier.padding(innerPadding)
                    )
                }*/
                QRScanner(sv)
            }
        }
    }

    override fun onResume() {
        super.onResume()
        scanner.startPreview()
    }

    override fun onPause() {
        scanner.releaseResources()
        super.onPause()
    }
}

@Composable
fun Greeting(name: String, modifier: Modifier = Modifier) {
    Text(
        text = "Hello $name!",
        modifier = modifier
    )
}

@Preview(showBackground = true)
@Composable
fun GreetingPreview() {
    SkipassScannerTheme {
        Greeting("Android")
    }
}

@Composable
fun QRScanner(sv: CodeScannerView)
{
    AndroidView(
        factory = {sv}
    )
}