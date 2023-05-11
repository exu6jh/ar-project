using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Custom matrix class
public class Matrix
{
    public float[,] values;

    public int getRows() {
        return values.GetLength(0);
    }

    public int getCols() {
        return values.GetLength(1);
    }

    public Matrix(Vector3 vector3) : this(new float[3, 1] { { vector3.x }, { vector3.y }, { vector3.z } }) { }

    public Matrix(float[,] values) {
        if(values.Rank != 2) {
            Debug.Log("Invalid size of multidimensional array; array must be 2-dimensional.");
            throw new System.ArithmeticException();
        }
        this.values = values;
    }

    public Matrix(string input) {
        // Remove starting or trailing brackets
        if(input.IndexOf("[") == 0) {
            input = input.Substring(1);
        }
        if(input.IndexOf("]") == input.Length - 1) {
            input = input.Substring(0, input.Length - 1);
        }
        // Create empty 2D array
        string[] rows = input.Split(';');
        string[] referenceCols = rows[0].Split(",");
        float[,] values = new float[rows.Length, referenceCols.Length];
        // Parse each line, checking for inconsistent size
        for(int i = 0; i < rows.Length; i++) {
            string[] cols = rows[i].Split(",");
            if(cols.Length != referenceCols.Length) {
                Debug.Log("Matrix size mismatch.");
                throw new System.ArithmeticException();
            }
            for(int j = 0; j < cols.Length; j++) {
                values[i,j] = float.Parse(cols[j]);
            }
        }
        this.values = values;
    }

    // Get transpose of matrix
    public Matrix Transpose() {
        float[,] tValues = new float[getCols(), getRows()];
        for(int i = 0; i < getCols(); i++) {
            for(int j = 0; i < getRows(); j++) {
                tValues[i,j] = values[j,i];
            }
        }
        return new Matrix(tValues);
    }

    // Get the dot product of two one-dimensional matrices
    public static float Dot(Matrix a, Matrix b) {
        if(a.getCols() != 1 || b.getCols() != 1 || a.getRows() != b.getRows()) {
            Debug.Log("Vector size mismatch. Both vectors must be n rows by 1 column.");
            throw new System.ArithmeticException();
        }
        Matrix single = a.Transpose() * b;
        return single.values[0,0];
    }

    // Get the magnitude of a one-dimensional matrix
    public float Magnitude() {
        return Mathf.Sqrt(Dot(this, this));
    }

    // Get the projection of a one-dimensional matrix
    public static Matrix Proj(Matrix u, Matrix a) {
        return(Dot(u, a) / Dot(u, u)) * u;
    }

    // TO BE IMPLEMENTED: determinant through QR factorization

    // Standard unary matrix negation operator
    public static Matrix operator -(Matrix a) {
        float [,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < a.getCols(); j++) {
                result[i,j] = -a.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix addition operator
    public static Matrix operator +(Matrix a, Matrix b) {
        if(a.getRows() != b.getRows() || a.getCols() != b.getCols()) {
            Debug.Log("Invalid dimensions of added matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a.values[i,j] + b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix subtraction operator
    public static Matrix operator -(Matrix a, Matrix b) {
        if(a.getRows() != b.getRows() || a.getCols() != b.getCols()) {
            Debug.Log("Invalid dimensions of added matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(), a.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a.values[i,j] - b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard scalar-matrix multiplication operator
    public static Matrix operator *(float a, Matrix b) {
        float[,] result = new float[b.getRows(), b.getCols()];
        for(int i = 0; i < b.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                result[i,j] = a * b.values[i,j];
            }
        }
        return new Matrix(result);
    }

    // Standard matrix-scalar multiplication operator
    public static Matrix operator *(Matrix a, float b) {
        return b * a;
    }

    // Standard matrix-matrix multiplication operator
    public static Matrix operator *(Matrix a, Matrix b) {
        if(a.getCols() != b.getRows()) {
            Debug.Log("Invalid dimensions of multiplied matrices.");
            throw new System.ArithmeticException();
        }

        float[,] result = new float[a.getRows(),b.getCols()];
        for(int i = 0; i < a.getRows(); i++) {
            for(int j = 0; j < b.getCols(); j++) {
                float val = 0;
                for(int k = 0; k < a.getCols(); k++) {
                    val += a.values[i,k] * b.values[k,j];
                }
                result[i,j] = val;
            }
        }
        return new Matrix(result);
    }

    // Custom ToString command to better examine matrices
    public override string ToString() {
        string mainString = "{";
        for(int i = 0; i < this.getRows(); i++) {
            for(int j = 0; j < this.getCols(); j++) {
                mainString += values[i,j].ToString() + (j != this.getCols() - 1 ? "," : "");
            }
            mainString += (i != this.getRows() - 1 ? ";" : "");
        }
        mainString += "}";
        return mainString;
    }
    
    public string ToStringSquareDelim() {
        string mainString = "[";
        for(int i = 0; i < this.getRows(); i++) {
            for(int j = 0; j < this.getCols(); j++) {
                mainString += values[i,j].ToString() + (j != this.getCols() - 1 ? "," : "");
            }
            mainString += (i != this.getRows() - 1 ? ";" : "");
        }
        mainString += "]";
        return mainString;
    }
    
    public static Matrix newRowVector(Vector3 vector3) => new(new[,] {{vector3[0], vector3[1], vector3[2]}});
}
