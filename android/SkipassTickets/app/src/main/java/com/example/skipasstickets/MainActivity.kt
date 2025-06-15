package com.example.skipasstickets

import android.content.Intent
import android.os.Bundle
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


class MainActivity : ComponentActivity() {
    var userId: Int? = null
    val launcher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) {result ->
        if (result.resultCode == RESULT_OK)
        {
            userId = result.data?.getIntExtra("userId", -1)
            Toast.makeText(this, userId.toString(), Toast.LENGTH_LONG).show()
            val qrEncoder = QRGEncoder(userId.toString(), QRGContents.Type.TEXT, 500)
            setContent {
                QRCode(qrEncoder.getBitmap(0).asImageBitmap())
            }
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

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
                QRCode(qrEncoder.getBitmap(0).asImageBitmap())
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