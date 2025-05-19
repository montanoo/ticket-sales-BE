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
import com.android.volley.toolbox.Volley
import com.budiyev.android.codescanner.AutoFocusMode
import com.budiyev.android.codescanner.CodeScanner
import com.budiyev.android.codescanner.CodeScannerView
import com.budiyev.android.codescanner.DecodeCallback
import com.budiyev.android.codescanner.ErrorCallback
import com.budiyev.android.codescanner.ScanMode
import com.example.skipassscanner.ui.theme.SkipassScannerTheme
import com.google.zxing.BarcodeFormat
import kotlin.system.exitProcess

class MainActivity : ComponentActivity() {
    private lateinit var scanner: CodeScanner

    private var requestQueue: RequestQueue = Volley.newRequestQueue(this)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

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
                // Some skeleton for request; uncomment below
                /*var request = JsonObjectRequest(
                // Don't know how to get the server set up for checking, probably need to run unit tests with a mock server
                Request.Method.GET, /*TODO: If you can test it, change the ellipsis to your url*/".../verify-mobile/" + it.text, null,
                { response ->
                    if (response.getBoolean("Valid"))
                    {
                        Toast.makeText(this, "Got a result", Toast.LENGTH_LONG).show()
                    }
                    else {
                        Toast.makeText(this, response.getString("Message"), Toast.LENGTH_LONG)
                            .show()
                    }
                },
                {
                    Toast.makeText(this, "Somthing went wrong", Toast.LENGTH_LONG).show()
                })
                requestQueue.add(request)*/
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