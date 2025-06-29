package com.example.skipasstickets

import android.app.Activity
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.widget.Toast
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.material3.Button
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import com.example.skipasstickets.ui.theme.SkipassTicketsTheme
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import androidx.compose.ui.text.input.PasswordVisualTransformation
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.JsonRequest
import com.android.volley.toolbox.Volley
import com.google.android.material.internal.ContextUtils.getActivity
import org.json.JSONObject

class LoginActivity : ComponentActivity() {
    private var requestQueue: RequestQueue? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        requestQueue = Volley.newRequestQueue(this)
        setContent {
            /*SkipassTicketsTheme {
                Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
                    Greeting(
                        name = "Android",
                        modifier = Modifier.padding(innerPadding)
                    )
                }
            }*/
            LoginForm(this, requestQueue)
        }
    }
}

@Composable
fun LoginForm(c: Context, rq: RequestQueue?)
{
    var email by remember { mutableStateOf("") }
    var pw by remember { mutableStateOf("") }
    Column {
        TextField(
            value = email,
            onValueChange = { updated ->
                email = updated
            },
            label = { Text("Email") }
        )
        TextField(
            value = pw,
            onValueChange = { updated ->
                pw = updated
            },
            label = { Text("Password") },
            visualTransformation = PasswordVisualTransformation()
        )
        Button(
            onClick = {
                val values = mapOf(Pair("email", email), Pair("password", pw))
                val request = JsonObjectRequest(
                    // NOTE: to get this to work, run your server with --urls="your_local_ip:port" param and change this to that ip and port
                    Request.Method.POST,
                    "http://192.168.1.19:5168/api/auth/login",
                    JSONObject(values),
                    { response ->
                        Toast.makeText(
                            c,
                            "Got a response: userId = " + response.getInt("userId"),
                            Toast.LENGTH_LONG
                        ).show()
                        val i = Intent()
                        i.putExtra("userId", response.getInt("userId"))
                        getActivity(c)?.setResult(Activity.RESULT_OK, i)
                        getActivity(c)?.finish()
                    },
                    { error ->
                        Toast.makeText(
                            c,
                            "Something went wrong: " + error.message,
                            Toast.LENGTH_LONG
                        ).show()
                    })
                rq?.add(request)
            }
        ) {
            Text(
                text = "Submit"
            )
        }
    }
}

/*@Composable
fun Greeting(name: String, modifier: Modifier = Modifier) {
    Text(
        text = "Hello $name!",
        modifier = modifier
    )
}*/

/*@Preview(showBackground = true)
@Composable
fun GreetingPreview() {
    SkipassTicketsTheme {
        Greeting("Android")
    }
}*/