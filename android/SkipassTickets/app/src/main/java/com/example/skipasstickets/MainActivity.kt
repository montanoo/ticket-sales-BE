package com.example.skipasstickets

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidmads.library.qrgenearator.QRGContents
import androidmads.library.qrgenearator.QRGEncoder
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.Image
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.ImageBitmap
import androidx.compose.ui.graphics.asImageBitmap
import com.example.skipasstickets.ui.theme.SkipassTicketsTheme
import androidx.compose.foundation.layout.Column
import androidx.compose.material3.Text
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley


class MainActivity : ComponentActivity() {
    var userId: Int? = null
    private var requestQueue: RequestQueue? = null
    val launcher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) {result ->
        if (result.resultCode == RESULT_OK)
        {
            userId = result.data?.getIntExtra("userId", -1)
            val qrEncoder = QRGEncoder(userId.toString(), QRGContents.Type.TEXT, 500)

            val request = JsonObjectRequest(
                // NOTE: to get this to work, run your server with --urls="your_local_ip:port" param and change this to that ip and port
                Request.Method.GET,
                "http://192.168.1.19:5168/api/tickets/verify-mobile/$userId", null,
                { response ->
                    if (response.getBoolean("valid"))
                    {
                        var text = "You have a valid ticket: expires " + response.getString("expiration")
                        if(!response.isNull("rideLimit"))
                        {
                            text += " (rides left: " + (response.getInt("rideLimit") - response.getInt("ridesTaken")) + ")"
                        }

                        setContent {
                            Column {
                                Text(text = text)
                                QRCode(qrEncoder.getBitmap(0).asImageBitmap())
                            }
                        }
                    }
                    else {
                        setContent {
                            Column {
                                Text(text = "No valid ticket available for this account")
                                QRCode(qrEncoder.getBitmap(0).asImageBitmap())
                            }
                        }
                    }
                },
                { error ->
                    setContent {
                        Column {
                            Text(text = "There was an error: " + error.message)
                        }
                    }
                })
            requestQueue?.add(request)
            setContent {
                Column {
                    Text(text = "Waiting...")
                }
            }
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        requestQueue = Volley.newRequestQueue(this)
        val i: Intent = Intent(applicationContext, LoginActivity::class.java)
        launcher.launch(i)

        var qrEncoder: QRGEncoder = QRGEncoder("there is an error in the QR code generator.", QRGContents.Type.TEXT, 500)

        enableEdgeToEdge()
        setContent {
            SkipassTicketsTheme {
                /*Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Greeting(
                        name = "Android",
                        modifier = Modifier.padding(innerPadding)
                    )
                }*/
                Column {
                    QRCode(qrEncoder.getBitmap(0).asImageBitmap())
                }
            }
        }
    }
}

@Composable
fun QRCode(bitmap: ImageBitmap)
{
    Image(
        bitmap = bitmap,
        contentDescription = "QR code"
    )
}

@Composable
fun ValidTicket()
{

}

/*@Composable
fun Greeting(name: String, modifier: Modifier = Modifier) {
    Text(
        text = "Hello $name!",
        modifier = modifier
    )
}

@Preview(showBackground = true)
@Composable
fun GreetingPreview() {
    SkipassTicketsTheme {
        Greeting("Android")
    }
}*/