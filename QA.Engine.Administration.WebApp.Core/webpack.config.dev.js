const webpack = require('webpack');
const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
    mode: 'development',
    entry: {
        vendor: '@babel/polyfill',
        main: './ClientApp/index.tsx'
    },
    output: {
        filename: '[name].bundle.js',
        path: path.join(__dirname, 'wwwroot/dist'),
        publicPath: '/'
    },
    devtool: 'source-map',
    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.json'],
        modules: ['node_modules'],
        alias: {
            components: path.resolve(__dirname, 'ClientApp/components/'),
            constants: path.resolve(__dirname, 'ClientApp/constants/'),
            enums: path.resolve(__dirname, 'ClientApp/enums/'),
            services: path.resolve(__dirname, 'ClientApp/services/'),
            stores: path.resolve(__dirname, 'ClientApp/stores/'),
            assets: path.resolve(__dirname, 'ClientApp/assets/'),
            qp: path.resolve(__dirname, 'ClientApp/qp/'),
        }
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader'
                }
            },
            {
                test: /\.css$/,
                use: [
                    { loader: 'style-loader' },
                    { loader: 'css-loader' }
                ],
            },
            {
                test: /\.(jpg|jpeg|png|gif|svg)?$/,
                exclude: /fonts/,
                use: {
                    loader: 'url-loader',
                    options: {
                        limit: 10000
                    }
                }
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf|svg)?$/,
                exclude: /img/,
                use: {
                    loader: 'url-loader',
                    options: {
                        limit: 10000
                    }
                }
            },
        ]
    },
    plugins: [
        new webpack.HotModuleReplacementPlugin(),
        new ForkTsCheckerWebpackPlugin(),
        new HtmlWebpackPlugin({
            title: 'Dev Mode',
            template: 'ClientApp/assets/index.html'
        }),
        CopyWebpackPlugin([{
            from: './ClientApp/assets/pmrpc.js',
            to: './scripts'
        }])
    ],
    optimization: {
        namedModules: true
    },
    devServer: {
        hot: true,
        port: 3000,
        proxy: {
            '/api': 'http://localhost:3001',
        },
        open: true,
        openPage: '?backend_sid=c3386b2f-e098-4dfb-a794-e774cba9fcfc&site_id=52&param_name=site_id&customerCode=qa_demosite&actionCode=custom_635494192491212659&hostUID=fc4a5aa1-48b9-4a3a-84c1-bcd99a8a8ff3'
    }
};
